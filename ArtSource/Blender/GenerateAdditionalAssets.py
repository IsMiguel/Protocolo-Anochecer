import bpy
import math
from mathutils import Vector

OUT = r"C:\Projectos de unity\Protocolo Anochecer\ArtSource\Blender"


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for block in list(bpy.data.collections):
        if block.name != "Collection":
            bpy.data.collections.remove(block)
    for block in list(bpy.data.materials):
        bpy.data.materials.remove(block)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1600
    scene.render.resolution_y = 1100
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.image_settings.color_depth = "8"
    scene.render.film_transparent = False
    scene.view_settings.view_transform = "AgX"
    scene.world.color = (0.035, 0.045, 0.055)
    return scene


def material(name, color, roughness=0.72, metallic=0.0, alpha=1.0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, alpha)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    nodes.clear()
    shader = nodes.new("ShaderNodeBsdfPrincipled")
    shader.inputs["Base Color"].default_value = (*color, alpha)
    shader.inputs["Roughness"].default_value = roughness
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Alpha"].default_value = alpha
    output = nodes.new("ShaderNodeOutputMaterial")
    mat.node_tree.links.new(shader.outputs["BSDF"], output.inputs["Surface"])
    if alpha < 1.0:
        mat.surface_render_method = "DITHERED"
    return mat


def collection(name, parent=None):
    col = bpy.data.collections.new(name)
    (parent or bpy.context.scene.collection).children.link(col)
    return col


def mesh_object(name, verts, faces, col, mat):
    mesh = bpy.data.meshes.new(name + " Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.materials.append(mat)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    col.objects.link(obj)
    return obj


def box(name, loc, dims, col, mat, bevel=0.0, rot=None):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    obj = bpy.context.object
    obj.name = name
    for c in list(obj.users_collection):
        c.objects.unlink(obj)
    col.objects.link(obj)
    obj.dimensions = dims
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if rot:
        obj.rotation_euler = rot
    if mat:
        obj.data.materials.append(mat)
    if bevel > 0:
        mod = obj.modifiers.new("Soft blockout edges", "BEVEL")
        mod.width = bevel
        mod.segments = 2
    return obj


def cylinder(name, a, b, radius, col, mat, vertices=20):
    a, b = Vector(a), Vector(b)
    delta = b - a
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=delta.length, location=(a + b) * 0.5)
    obj = bpy.context.object
    obj.name = name
    for c in list(obj.users_collection):
        c.objects.unlink(obj)
    col.objects.link(obj)
    obj.rotation_euler = delta.to_track_quat("Z", "Y").to_euler()
    obj.data.materials.append(mat)
    return obj


def sphere(name, loc, scale, col, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=20, ring_count=12, location=loc)
    obj = bpy.context.object
    obj.name = name
    for c in list(obj.users_collection):
        c.objects.unlink(obj)
    col.objects.link(obj)
    obj.scale = scale
    obj.data.materials.append(mat)
    bpy.ops.object.shade_smooth()
    return obj


def aim_camera(scene, loc, target, ortho, name="Preview Camera"):
    data = bpy.data.cameras.new(name)
    camera = bpy.data.objects.new(name, data)
    scene.collection.objects.link(camera)
    camera.location = loc
    camera.rotation_euler = (Vector(target) - Vector(loc)).to_track_quat("-Z", "Y").to_euler()
    data.type = "ORTHO"
    data.ortho_scale = ortho
    scene.camera = camera
    return camera


def area_light(scene, name, loc, energy, size):
    data = bpy.data.lights.new(name, "AREA")
    data.energy = energy
    data.shape = "DISK"
    data.size = size
    obj = bpy.data.objects.new(name, data)
    scene.collection.objects.link(obj)
    obj.location = loc
    obj.rotation_euler = (Vector((0, 0, 1)) - obj.location).to_track_quat("-Z", "Y").to_euler()


def studio_render(scene, blend_path, render_path):
    scene.render.filepath = render_path
    scene.render.engine = "BLENDER_EEVEE"
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    # Workbench gives a clean, readable color-block preview while keeping the
    # editable .blend configured for Eevee materials and lighting.
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.color_type = "MATERIAL"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.studio_light = "paint.sl"
    scene.display.shading.background_type = "WORLD"
    scene.display.shading.show_shadows = True
    scene.display.shading.show_cavity = True
    scene.display.shading.cavity_type = "BOTH"
    scene.display.shading.curvature_ridge_factor = 1.25
    scene.display.shading.curvature_valley_factor = 0.9
    bpy.ops.render.render(write_still=True)
    scene.render.engine = "BLENDER_EEVEE"
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)


def build_helicopter():
    scene = reset_scene()
    main = collection("Bell UH-1H Iroquois - BLOCKOUT")
    main["status"] = "Provisional external game asset; metric scale; separate editable components."
    main["reference"] = "UH-1H proportions guided by Smithsonian and U.S. Army published dimensions."
    main["note"] = "Postwar aircraft, kept separate from the 1940s opening sequence. Surface detail and UVs pending."
    olive = material("Blockout - olive drab", (0.20, 0.235, 0.19), 0.8)
    olive_light = material("Blockout - sun-faded olive", (0.29, 0.32, 0.25), 0.78)
    metal = material("Blockout - dark steel", (0.075, 0.085, 0.08), 0.45, 0.4)
    rubber = material("Blockout - skid rubber", (0.025, 0.03, 0.03), 0.88)
    glass = material("Blockout - tinted glazing", (0.045, 0.12, 0.15), 0.19, 0.25, 0.64)
    interior = material("Blockout - cabin interior", (0.085, 0.09, 0.075), 0.87)
    rotor_mat = material("Blockout - rotor composite", (0.095, 0.11, 0.095), 0.68)

    # Main cabin shell: fuselage fore-aft axis is +X; Z is up.
    sections = [
        (4.95, 0.32, 1.58, 0.34), (4.35, 0.73, 1.57, 0.66),
        (3.35, 0.98, 1.63, 0.88), (2.0, 1.03, 1.66, 0.96),
        (0.0, 1.02, 1.68, 0.98), (-2.1, 0.94, 1.62, 0.91),
        (-3.4, 0.72, 1.55, 0.70), (-4.05, 0.43, 1.51, 0.46),
    ]
    ring_n = 16
    verts, faces = [], []
    for x, ry, cz, rz in sections:
        for i in range(ring_n):
            a = 2 * math.pi * i / ring_n
            verts.append((x, math.cos(a) * ry, cz + math.sin(a) * rz))
    for r in range(len(sections) - 1):
        for i in range(ring_n):
            j = (i + 1) % ring_n
            faces.append((r * ring_n + i, r * ring_n + j, (r + 1) * ring_n + j, (r + 1) * ring_n + i))
    faces.extend([tuple(range(ring_n - 1, -1, -1)), tuple((len(sections)-1)*ring_n + i for i in range(ring_n))])
    body = mesh_object("Fuselage - primary shell", verts, faces, main, olive)
    for p in body.data.polygons:
        p.use_smooth = True

    # Tail boom, fin and stabilizer.
    cylinder("Tail boom - tapered silhouette", (-3.4, 0, 1.55), (-7.65, 0, 1.63), 0.22, main, olive, 24)
    sphere("Tail gearbox fairing", (-7.45, 0, 1.68), (0.45, 0.38, 0.38), main, olive_light)
    box("Vertical tail fin", (-7.15, 0, 2.35), (1.05, 0.12, 1.35), main, olive, 0.08, (0, math.radians(-13), 0))
    box("Tail horizontal stabilizer", (-6.25, 0, 2.16), (1.85, 2.2, 0.14), main, olive_light, 0.05)

    # Cockpit glazing and cabin windows on both sides.
    for side in (-1, 1):
        y = side * 0.905
        for idx, x in enumerate((1.8, 0.85, -0.1, -1.05, -2.0)):
            box("Cabin window %s %02d" % ("L" if side < 0 else "R", idx + 1),
                (x, side * 0.99, 2.23), (0.72, 0.035, 0.48), main, glass, 0.045)
        # Sliding-door seams and lower sill rails.
        box("Cabin sliding door panel %s" % side, (-0.55, side * 0.92, 1.64), (2.05, 0.025, 0.94), main, olive, 0.025)
        box("Door lower track %s" % side, (-0.55, side * 0.99, 1.02), (2.34, 0.04, 0.045), main, metal, 0.012)
        box("Door upper track %s" % side, (-0.55, side * 0.99, 2.26), (2.34, 0.04, 0.045), main, metal, 0.012)
        box("Door handle %s" % side, (-1.42, side * 1.015, 1.52), (0.12, 0.025, 0.065), main, metal, 0.015)
        # Small rear cabin porthole.
        bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=0.22, depth=0.035, location=(-2.9, side * 0.75, 1.9), rotation=(math.pi/2, 0, 0))
        port = bpy.context.object
        port.name = "Rear cabin porthole %s" % side
        for c in list(port.users_collection): c.objects.unlink(port)
        main.objects.link(port)
        port.data.materials.append(glass)
        # Cabin bench silhouettes.
        box("Cabin bench frame %s" % side, (-0.4, side * 0.40, 1.15), (2.25, 0.12, 0.12), main, metal, 0.025)
        box("Cabin bench cushion %s" % side, (-0.4, side * 0.40, 1.29), (2.2, 0.22, 0.18), main, interior, 0.05)

    # Split front windscreens set ahead of the cockpit.
    for side in (-1, 1):
        box("Pilot windscreen %s" % side, (4.25, side * 0.37, 2.30), (0.06, 0.67, 0.64), main, glass, 0.04, (0, math.radians(-20), 0))
    box("Windscreen center post", (4.29, 0, 2.31), (0.10, 0.07, 0.73), main, olive_light, 0.025)
    box("Instrument panel", (3.80, 0, 1.82), (0.32, 1.25, 0.28), main, interior, 0.045)
    for side in (-1, 1):
        box("Pilot seat cushion %s" % side, (3.20, side * 0.36, 1.22), (0.56, 0.50, 0.16), main, interior, 0.055)
        box("Pilot seat back %s" % side, (3.42, side * 0.36, 1.60), (0.16, 0.50, 0.62), main, interior, 0.055, (0, math.radians(-8), 0))

    # Engine cowling, paired exhausts and roof intake.
    box("Engine cowling", (-0.25, 0, 2.66), (2.10, 0.94, 0.47), main, olive_light, 0.16)
    box("Engine intake lip", (0.25, 0, 2.92), (0.75, 0.82, 0.09), main, metal, 0.035)
    for side in (-1, 1):
        cylinder("Engine exhaust %s" % side, (-1.02, side * 0.53, 2.67), (-1.75, side * 0.59, 2.58), 0.115, main, metal, 20)
        box("Engine access panel %s" % side, (-0.18, side * 0.52, 2.42), (0.95, 0.04, 0.24), main, olive, 0.035)

    # Main rotor mast, hub, two full-size blades and pitch links.
    cylinder("Main rotor mast", (0, 0, 2.82), (0, 0, 4.0), 0.105, main, metal)
    cylinder("Main rotor hub", (0, 0, 3.88), (0, 0, 4.17), 0.31, main, olive_light)
    cylinder("Hub crosshead", (-0.65, 0, 4.02), (0.65, 0, 4.02), 0.095, main, metal)
    for index, angle in enumerate((0, math.pi)):
        # Flat tapered blade extending 0.65m to 7.35m from hub.
        outline = [(0.48, -0.13), (1.05, -0.265), (6.85, -0.255), (7.35, -0.10),
                   (7.35, 0.10), (6.85, 0.255), (1.05, 0.265), (0.48, 0.13)]
        blade_verts = []
        for z in (4.08, 4.17):
            blade_verts.extend((x, y, z) for x, y in outline)
        blade_faces = [tuple(range(7, -1, -1)), tuple(range(8, 16))]
        for i in range(8):
            j = (i + 1) % 8
            blade_faces.append((i, j, j + 8, i + 8))
        blade = mesh_object("Main rotor blade %02d" % (index+1), blade_verts, blade_faces, main, rotor_mat)
        blade.rotation_euler[2] = angle
        blade.location = (0, 0, 0)
        cylinder("Main rotor grip %02d" % (index+1), (0, 0, 4.1), (0.95 * math.cos(angle), 0.95 * math.sin(angle), 4.1), 0.09, main, metal)

    # Tail rotor shaft and two blades on the port side.
    cylinder("Tail rotor shaft", (-7.50, -0.32, 1.83), (-7.50, -0.72, 1.83), 0.075, main, metal)
    sphere("Tail rotor hub", (-7.50, -0.76, 1.83), (0.14, 0.11, 0.14), main, olive_light)
    for idx, angle in enumerate((math.radians(42), math.radians(222))):
        blade = box("Tail rotor blade %02d" % (idx+1), (-7.50, -0.83, 1.83), (0.10, 0.12, 1.72), main, rotor_mat, 0.035)
        blade.rotation_euler[0] = angle

    # Landing skids and struts.
    for side in (-1, 1):
        y = side * 1.02
        cylinder("Landing skid rail %s" % side, (3.25, y, 0.39), (-3.65, y, 0.39), 0.085, main, rubber, 20)
        cylinder("Forward skid upstand %s" % side, (2.05, y, 0.43), (1.55, side * 0.77, 1.14), 0.055, main, metal)
        cylinder("Aft skid upstand %s" % side, (-1.95, y, 0.43), (-1.40, side * 0.77, 1.18), 0.055, main, metal)
    cylinder("Skid forward cross tube", (2.0, -1.07, 0.51), (2.0, 1.07, 0.51), 0.055, main, metal)
    cylinder("Skid aft cross tube", (-1.9, -1.07, 0.51), (-1.9, 1.07, 0.51), 0.055, main, metal)

    # Exterior detailing: service handles, lights, antennas and tie points.
    for side in (-1, 1):
        box("Cabin step %s" % side, (1.10, side * 1.14, 0.93), (0.68, 0.38, 0.07), main, metal, 0.025)
        sphere("Nose position lamp %s" % side, (4.72, side * 0.50, 1.94), (0.10, 0.08, 0.07), main, olive_light)
        cylinder("Roof antenna %s" % side, (-2.9, side * 0.23, 2.70), (-3.0, side * 0.23, 3.14), 0.018, main, metal, 12)
    box("Aft belly fairing", (-2.40, 0, 0.98), (0.88, 0.62, 0.22), main, olive, 0.08)
    box("Roof aerial", (1.55, 0, 2.65), (0.52, 0.055, 0.045), main, metal, 0.015, (0, math.radians(-8), 0))

    # Studio only: separate from the asset collection.
    studio = collection("Presentation - camera and lights")
    box("Studio ground", (0, 0, -0.18), (46, 42, 0.18), studio, material("Studio floor", (0.055, 0.065, 0.07), 0.9))
    aim_camera(scene, (15.8, -20.5, 11.8), (0, 0, 2.2), 19.2, "UH-1H three-quarter preview camera")
    area_light(scene, "Large softbox", (3, -10, 15), 1200, 10)
    area_light(scene, "Tail rim", (-10, 5, 9), 900, 8)
    area_light(scene, "Nose fill", (12, 3, 6), 500, 7)
    studio_render(scene, OUT + r"\Bell_UH1H_Prototype.blend", OUT + r"\Bell_UH1H_Preview.png")


def building(name, center, size, height, col, wall, roof):
    x, y = center
    sx, sy = size
    group = collection(name, col)
    box(name + " - plinth", (x, y, 0.14), (sx + 0.25, sy + 0.25, 0.28), group, material("Building plinth", (0.40, 0.37, 0.31), 0.92), 0.04)
    box(name + " - walls", (x, y, 0.28 + height/2), (sx, sy, height), group, wall, 0.08)
    # Low pitched, broad overhanging roof assembled from two sloping planes.
    half = sx * 0.53
    ridge_z = 0.28 + height + min(1.25, sy * 0.12)
    eave_z = 0.28 + height + 0.12
    roof_half_y = sy * 0.58
    for side in (-1, 1):
        center_y = y + side * roof_half_y/2
        slope = math.atan2(ridge_z - eave_z, roof_half_y/2)
        box(name + " - roof slope %s" % side, (x, center_y, (ridge_z + eave_z)/2), (sx + 0.8, roof_half_y, 0.16), group, roof, 0.035, (side * slope, 0, 0))
    box(name + " - ridge cap", (x, y, ridge_z + 0.04), (sx + 0.88, 0.18, 0.12), group, roof, 0.04)
    # Repeated shaded window bays on all exposed long walls.
    count = max(2, int(sx / 2.6))
    for side in (-1, 1):
        for idx in range(count):
            wx = x - sx/2 + (idx + 0.5) * sx/count
            box(name + " - window %s %02d" % (side, idx+1), (wx, y + side*(sy/2 + 0.012), 0.28 + height*0.63), (min(1.05, sx/count*0.55), 0.045, 0.65), group, CAMPUS_GLASS, 0.025)
            box(name + " - sill %s %02d" % (side, idx+1), (wx, y + side*(sy/2 + 0.045), 0.28 + height*0.63 - 0.35), (min(1.2, sx/count*0.62), 0.10, 0.08), group, CAMPUS_TRIM, 0.015)
    # Main entrance and veranda awning on the front-facing side.
    box(name + " - main door", (x, y - sy/2 - 0.035, 0.28 + 1.04), (1.05, 0.07, 2.05), group, CAMPUS_DOOR, 0.025)
    box(name + " - veranda canopy", (x, y - sy/2 - 0.9, 0.28 + height - 0.18), (sx + 0.4, 1.7, 0.18), group, roof, 0.04)
    veranda_posts = max(3, int(sx/3.5))
    for idx in range(veranda_posts):
        px = x - sx/2 + idx * sx/(veranda_posts-1)
        box(name + " - veranda post %02d" % (idx+1), (px, y - sy/2 - 1.45, 0.28 + (height - 0.18)/2), (0.16, 0.16, height - 0.18), group, CAMPUS_TRIM, 0.025)
    # Shallow base-course stripe makes the blockout easier to texture later.
    box(name + " - base course", (x, y - sy/2 - 0.02, 0.50), (sx, 0.045, 0.28), group, CAMPUS_BRICK, 0.01)
    return group


def tree(col, x, y, height=5.0, spread=2.4):
    trunk = material("Tree trunk brown", (0.22, 0.14, 0.08), 0.9)
    leaves = material("Chaco shade tree foliage", (0.17, 0.29, 0.14), 0.95)
    cylinder("Tree trunk", (x, y, 0), (x, y, height*0.52), 0.19, col, trunk, 12)
    for i, offset in enumerate(((0,0,0),(spread*.38,0,height*.10),(-spread*.33,.12,height*.05),(0,-spread*.32,height*.12))):
        sphere("Canopy cluster %02d" % (i+1), (x+offset[0], y+offset[1], height*.63+offset[2]), (spread*.56, spread*.48, height*.34), col, leaves)


def build_school():
    global CAMPUS_GLASS, CAMPUS_TRIM, CAMPUS_DOOR, CAMPUS_BRICK
    scene = reset_scene()
    campus = collection("Centro de Formación Militar Chaqueño - FICTIONAL CONCEPT")
    campus["status"] = "Fictional training campus concept inspired by Chaco climate and regional institutional architecture."
    campus["note"] = "Not a measured copy of an active school. Simplified architectural blockout for later Blender detailing and texturing."
    plaster = material("Warm whitewashed plaster", (0.69, 0.65, 0.54), 0.92)
    pale = material("Ivory exterior trim", (0.83, 0.78, 0.64), 0.88)
    roof = material("Corrugated roof blockout", (0.26, 0.29, 0.27), 0.82, 0.12)
    clay = material("Dusty red earth", (0.32, 0.21, 0.13), 0.98)
    asphalt = material("Driveway asphalt", (0.12, 0.13, 0.12), 0.96)
    concrete = material("Pale concrete", (0.53, 0.52, 0.46), 0.95)
    CAMPUS_GLASS = material("Dark green window glass", (0.075, 0.14, 0.13), 0.32, 0.12)
    CAMPUS_TRIM = pale
    CAMPUS_DOOR = material("Painted timber doors", (0.24, 0.18, 0.12), 0.84)
    CAMPUS_BRICK = material("Red brick course", (0.36, 0.21, 0.14), 0.96)

    # Entire fictional compound is set on a 110 x 92m planning grid.
    box("Campus site - Chaco earth", (0, 0, -0.28), (112, 94, 0.55), campus, clay)
    box("Perimeter north wall", (0, 45, 1.2), (112, 0.5, 2.4), campus, concrete, 0.08)
    box("Perimeter west wall", (-55, 0, 1.2), (0.5, 90, 2.4), campus, concrete, 0.08)
    box("Perimeter east wall", (55, 0, 1.2), (0.5, 90, 2.4), campus, concrete, 0.08)
    box("Perimeter south wall west", (-27, -45, 1.2), (56, 0.5, 2.4), campus, concrete, 0.08)
    box("Perimeter south wall east", (27, -45, 1.2), (56, 0.5, 2.4), campus, concrete, 0.08)

    # Parade ground and paths.
    box("Central parade ground", (0, 1, 0.015), (37, 29, 0.10), campus, material("Parade ground gravel", (0.57, 0.49, 0.36), 0.98))
    box("Front access road", (0, -38, 0.03), (14, 13, 0.08), campus, asphalt)
    box("Main central path", (0, -19, 0.06), (5.5, 24, 0.12), campus, concrete)
    box("North cross path", (0, 18, 0.055), (61, 4.0, 0.11), campus, concrete)
    box("West cross path", (-25, 0, 0.055), (4.0, 38, 0.11), campus, concrete)
    box("East cross path", (25, 0, 0.055), (4.0, 38, 0.11), campus, concrete)
    # Parade boundary stripes and central assembly marker.
    for x in (-17.8, 17.8):
        box("Parade ground boundary stripe", (x, 1, 0.075), (0.13, 27.8, 0.025), campus, pale)
    for y in (-12.8, 14.8):
        box("Parade ground boundary stripe", (0, y, 0.075), (35.7, 0.13, 0.025), campus, pale)
    cylinder("Parade assembly circle", (0, 0, 0.09), (0, 0, 0.115), 3.2, campus, pale, 48)

    # Main entrance and guard post.
    building("Guardhouse - south entrance", (-10.5, -40.2), (7, 5), 2.9, campus, plaster, roof)
    building("Guardhouse - vehicle control", (10.5, -40.2), (7, 5), 2.9, campus, plaster, roof)
    box("Main gate lintel", (0, -44.7, 3.4), (14.5, 0.65, 0.7), campus, pale, 0.08)
    box("Vehicle gate left leaf", (-3.1, -44.7, 1.5), (5.9, 0.18, 2.7), campus, metal_material(), 0.03)
    box("Vehicle gate right leaf", (3.1, -44.7, 1.5), (5.9, 0.18, 2.7), campus, metal_material(), 0.03)
    box("Entrance sign panel", (0, -44.85, 4.15), (8.7, 0.22, 0.7), campus, CAMPUS_BRICK, 0.05)
    # Sign text is editable text geometry, deliberately generic and fictional.
    text_data = bpy.data.curves.new("Fictional campus sign lettering", "FONT")
    text_data.body = "CENTRO DE FORMACION  -  CHACO"
    text_data.align_x = "CENTER"
    text_data.size = 0.30
    text_obj = bpy.data.objects.new("Generic fictional campus sign text", text_data)
    campus.objects.link(text_obj)
    text_obj.location = (0, -45.0, 4.05)
    text_obj.rotation_euler = (math.radians(90), 0, 0)
    text_obj.data.materials.append(pale)

    # Teaching, accommodation, dining and assembly blocks arranged around the drill yard.
    building("Administration and reception", (0, -28.0), (21, 9), 3.8, campus, plaster, roof)
    building("Classrooms east", (35, -3), (19, 10), 3.5, campus, plaster, roof)
    building("Barracks west", (-37, -4), (20, 12), 3.5, campus, plaster, roof)
    building("Dining hall and kitchen", (0, 29), (22, 11), 3.8, campus, plaster, roof)
    building("Assembly hall", (37, 27), (20, 13), 4.3, campus, plaster, roof)
    building("Infirmary", (35, -23), (15, 8), 3.1, campus, plaster, roof)
    building("Stores and workshops", (-37, 25), (22, 10), 3.4, campus, plaster, roof)

    # Covered sports court and simple outdoor training stations.
    box("Sports court surface", (-29, 8, 0.06), (21, 12, 0.12), campus, concrete)
    for x in (-39.5, -18.5):
        for y in (2.5, 13.5):
            cylinder("Sports court roof post", (x, y, 0.1), (x, y, 6.1), 0.14, campus, CAMPUS_TRIM, 12)
    box("Sports court roof", (-29, 8, 6.2), (22.2, 12.8, 0.2), campus, roof, 0.04)
    for x in (-29, -19):
        box("Court line", (x, 8, 0.13), (0.08, 11, 0.025), campus, pale)
    box("Court center line", (-29, 8, 0.13), (20.5, 0.08, 0.025), campus, pale)
    # Drill/fitness area with low timber obstacles and climbing frames.
    box("Outdoor drill yard", (-37, -26, 0.035), (24, 11, 0.07), campus, material("Drill yard sand", (0.50, 0.39, 0.26), 0.98))
    for idx, x in enumerate((-46, -40, -34, -28)):
        box("Low training beam %02d" % idx, (x, -26, 0.44), (3.7, 0.18, 0.18), campus, CAMPUS_DOOR, 0.045)
        for dx in (-1.55, 1.55):
            box("Training beam foot", (x+dx, -26, 0.22), (0.16, 0.20, 0.45), campus, CAMPUS_DOOR, 0.025)
    for i in range(3):
        cylinder("Fitness frame upright", (-47+i*4, -33, 0.1), (-47+i*4, -33, 2.2), 0.065, campus, metal_material(), 12)
    for i in range(2):
        cylinder("Fitness frame crossbar", (-47, -33, 1.55+i*0.35), (-39, -33, 1.55+i*0.35), 0.055, campus, metal_material(), 12)

    # Flagpole and low platform in the parade ground.
    box("Flagpole plinth", (0, -5, 0.20), (2.1, 2.1, 0.35), campus, concrete, 0.08)
    cylinder("Campus flagpole", (0, -5, 0.28), (0, -5, 12.5), 0.075, campus, metal_material(), 20)
    box("Unmarked ceremonial flag", (1.05, -5, 11.4), (2.0, 0.07, 1.15), campus, material("Ceremonial flag placeholder", (0.53, 0.40, 0.18), 0.75))

    # Regionally appropriate shade planting, water tank and benches.
    for x, y in ((-49,-36),(-48,-16),(-50,8),(-48,35),(49,-35),(49,-14),(49,9),(49,34),(-16,-18),(17,-18),(-15,19),(17,20)):
        tree(campus, x, y, height=4.6, spread=1.85)
    cylinder("Elevated water tower legs", (46, 37, 0), (46, 37, 9.1), 0.17, campus, metal_material(), 16)
    for dx, dy in ((-1.2,-1.2),(1.2,-1.2),(-1.2,1.2),(1.2,1.2)):
        cylinder("Water tower support", (46+dx, 37+dy, 0), (46, 37, 7.1), 0.12, campus, metal_material(), 12)
    cylinder("Water tank", (46, 37, 7.3), (46, 37, 9.4), 1.55, campus, roof, 32)
    for x, y in ((-8, -15), (8, -15), (-8, 17), (8, 17), (-43, 0), (43, 0)):
        box("Courtyard bench seat", (x, y, 0.55), (2.1, 0.48, 0.12), campus, CAMPUS_DOOR, 0.04)
        for legx in (-0.78, 0.78):
            box("Bench leg", (x+legx, y, 0.30), (0.13, 0.16, 0.48), campus, metal_material(), 0.02)

    studio = collection("Presentation - camera and lights")
    aim_camera(scene, (78, -92, 115), (0, 0, 1), 165, "Campus overview preview camera")
    area_light(scene, "Campus softbox", (0, -42, 95), 9000, 55)
    area_light(scene, "Campus fill", (-55, 35, 65), 5500, 42)
    studio_render(scene, OUT + r"\Chaco_Inspired_Military_Campus_Concept.blend", OUT + r"\Chaco_Campus_Preview.png")


def metal_material():
    # Shared metal blockout swatch used by the campus detailing.
    mat = bpy.data.materials.get("Campus dark metal")
    if mat is None:
        mat = material("Campus dark metal", (0.10, 0.11, 0.10), 0.62, 0.24)
    return mat


build_helicopter()
build_school()
