import bpy
import math
from mathutils import Vector

OUTPUT_DIR = r"C:\Projectos de unity\Protocolo Anochecer\ArtSource\Blender"
BLEND_PATH = OUTPUT_DIR + r"\Weapons_1940s_Prototype.blend"
RENDER_PATH = OUTPUT_DIR + r"\Weapons_1940s_Preview.png"

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
for collection in list(bpy.data.collections):
    if collection.name != "Collection":
        bpy.data.collections.remove(collection)

scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1.0
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = 1800
scene.render.resolution_y = 1050
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.film_transparent = False
scene.render.image_settings.color_mode = "RGBA"
scene.world.color = (0.055, 0.06, 0.065)

def make_material(name, color, roughness=0.72, metallic=0.0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    nodes.clear()
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    output = nodes.new("ShaderNodeOutputMaterial")
    mat.node_tree.links.new(bsdf.outputs["BSDF"], output.inputs["Surface"])
    return mat
wood = make_material("Blockout - walnut wood", (0.28, 0.17, 0.09))
metal = make_material("Blockout - dark parkerized metal", (0.095, 0.11, 0.12), 0.42, 0.45)
steel = make_material("Blockout - worn steel", (0.25, 0.27, 0.28), 0.38, 0.62)
rubber = make_material("Blockout - dark rubber", (0.045, 0.05, 0.05), 0.82)
brass = make_material("Blockout - muted brass", (0.42, 0.32, 0.13), 0.48, 0.55)

root_collection = bpy.data.collections.new("Arsenal 1940s - PROVISIONAL")
scene.collection.children.link(root_collection)
root_collection["status"] = "Provisional visual props; external silhouettes only, no internal mechanisms."
root_collection["note"] = "Historically inspired game assets. Confirm final canon and variants before production."

def link_to(obj, collection):
    for current in list(obj.users_collection):
        current.objects.unlink(obj)
    collection.objects.link(obj)

def new_weapon(name, position, index):
    collection = bpy.data.collections.new(name)
    root_collection.children.link(collection)
    root = bpy.data.objects.new(name + " - ROOT", None)
    collection.objects.link(root)
    root.location = position
    root.empty_display_type = "PLAIN_AXES"
    root.empty_display_size = 0.14
    root["asset_status"] = "PROVISIONAL"
    root["scale_reference"] = "Approximate external game-prop proportions; retopology and final dimensions pending."
    return root, collection

def add_cube(root, collection, name, location, dimensions, material, bevel=0.012, rotation=None):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0.0, 0.0, 0.0))
    obj = bpy.context.object
    obj.name = name
    link_to(obj, collection)
    obj.parent = root
    obj.location = location
    obj.scale = dimensions
    if rotation is not None:
        obj.rotation_euler = rotation
    if material:
        obj.data.materials.append(material)
    if bevel > 0:
        modifier = obj.modifiers.new("Soft manufactured edges", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
    return obj

def add_cylinder(root, collection, name, location, radius, length, material, axis="X", vertices=20):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=1.0, depth=1.0, location=(0.0, 0.0, 0.0))
    obj = bpy.context.object
    obj.name = name
    link_to(obj, collection)
    obj.parent = root
    obj.location = location
    obj.scale = (radius, radius, length * 0.5)
    if axis == "X":
        obj.rotation_euler[1] = math.pi * 0.5
    elif axis == "Y":
        obj.rotation_euler[0] = math.pi * 0.5
    if material:
        obj.data.materials.append(material)
    bevel = obj.modifiers.new("Edge highlights", "BEVEL")
    bevel.width = min(radius * 0.15, 0.008)
    bevel.segments = 2
    return obj

def add_profile(root, collection, name, profile, thickness, material):
    vertices = []
    for y in (-thickness * 0.5, thickness * 0.5):
        vertices.extend((x, y, z) for x, z in profile)
    count = len(profile)
    faces = [tuple(range(count - 1, -1, -1)), tuple(range(count, count * 2))]
    for i in range(count):
        nxt = (i + 1) % count
        faces.append((i, nxt, nxt + count, i + count))
    mesh = bpy.data.meshes.new(name + " Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.materials.append(material)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    obj.parent = root
    return obj

def long_rifle(name, position, kind):
    root, col = new_weapon(name, position, 0)
    if kind in ("mauser", "kar98"):
        stock_end = -0.60 if kind == "mauser" else -0.56
        add_profile(root, col, "Wood stock", [
            (-1.12, -0.10), (-1.08, 0.13), (-0.62, 0.12),
            (-0.28, 0.09), (0.04, 0.045), (0.04, -0.11),
            (-0.16, -0.19), (-0.62, -0.18), (-0.95, -0.15)
        ], 0.13, wood)
        add_cube(root, col, "Fore-end", (0.28, 0.0, -0.015), (0.66, 0.105, 0.105), wood, 0.025)
        barrel_start, barrel_end = (0.22, 1.32) if kind == "mauser" else (0.20, 1.23)
        add_cube(root, col, "Receiver", (0.01, 0.0, 0.01), (0.46, 0.115, 0.13), metal, 0.025)
        add_cylinder(root, col, "Barrel", ((barrel_start + barrel_end) / 2, 0.0, 0.035), 0.022, barrel_end - barrel_start, steel)
        add_cylinder(root, col, "Muzzle band", (barrel_end - 0.025, 0.0, 0.035), 0.031, 0.05, metal)
        add_cube(root, col, "Rear sight block", (0.36, 0.0, 0.105), (0.10, 0.08, 0.07), steel, 0.008)
        add_cube(root, col, "Front sight", (barrel_end - 0.10, 0.0, 0.085), (0.035, 0.045, 0.07), steel, 0.004)
        add_cube(root, col, "Bolt body", (-0.02, -0.075, 0.055), (0.28, 0.035, 0.04), steel, 0.008)
        add_cylinder(root, col, "Bolt handle", (0.04, -0.095, -0.02), 0.018, 0.11, steel, axis="Z", vertices=12)
        add_cylinder(root, col, "Bolt knob", (0.04, -0.095, -0.08), 0.032, 0.045, steel, axis="Z", vertices=12)
        add_cube(root, col, "Magazine", (0.05, 0.0, -0.15), (0.16, 0.08, 0.09), metal, 0.012)
        add_cube(root, col, "Trigger guard", (-0.19, 0.0, -0.13), (0.22, 0.045, 0.035), steel, 0.012)
        add_cube(root, col, "Trigger", (-0.18, 0.0, -0.09), (0.025, 0.018, 0.07), steel, 0.003)
        add_cube(root, col, "Sling swivel front", (0.40, 0.0, -0.105), (0.08, 0.03, 0.025), steel, 0.004)
        add_cube(root, col, "Sling swivel rear", (-0.82, 0.0, -0.16), (0.08, 0.03, 0.025), steel, 0.004)
        if kind == "kar98":
            add_cube(root, col, "Upper handguard", (0.28, 0.0, 0.067), (0.70, 0.075, 0.045), wood, 0.012)
        else:
            add_cube(root, col, "Upper handguard", (0.42, 0.0, 0.067), (0.52, 0.075, 0.045), wood, 0.012)
    else:
        add_profile(root, col, "Garand wood stock", [
            (-1.08, -0.12), (-1.06, 0.14), (-0.64, 0.13),
            (-0.30, 0.08), (0.12, 0.055), (0.10, -0.12),
            (-0.12, -0.19), (-0.56, -0.18), (-0.90, -0.16)
        ], 0.14, wood)
        add_cube(root, col, "Upper handguard rear", (0.28, 0.0, 0.07), (0.46, 0.095, 0.075), wood, 0.018)
        add_cube(root, col, "Upper handguard front", (0.69, 0.0, 0.06), (0.32, 0.085, 0.065), wood, 0.014)
        add_cube(root, col, "Garand receiver", (0.02, 0.0, 0.015), (0.52, 0.12, 0.14), metal, 0.025)
        add_cylinder(root, col, "Garand barrel", (0.73, 0.0, 0.025), 0.021, 1.02, steel)
        add_cylinder(root, col, "Gas cylinder", (0.55, 0.0, -0.07), 0.035, 0.43, metal)
        add_cube(root, col, "Rear sight", (-0.07, 0.0, 0.115), (0.12, 0.09, 0.08), steel, 0.008)
        add_cube(root, col, "Front sight", (1.12, 0.0, 0.08), (0.045, 0.055, 0.075), steel, 0.004)
        add_cube(root, col, "En bloc clip silhouette", (0.10, 0.0, -0.13), (0.12, 0.075, 0.10), brass, 0.008)
        add_cube(root, col, "Trigger guard", (-0.22, 0.0, -0.13), (0.24, 0.045, 0.035), steel, 0.012)
        add_cube(root, col, "Trigger", (-0.20, 0.0, -0.09), (0.025, 0.018, 0.07), steel, 0.003)
    return root

def add_submachine_gun(name, position, kind):
    root, col = new_weapon(name, position, 0)
    add_cube(root, col, "Main receiver", (-0.02, 0.0, 0.02), (0.58, 0.13, 0.16), metal, 0.03)
    if kind == "thompson":
        add_profile(root, col, "Wood buttstock", [
            (-0.97, -0.07), (-0.94, 0.12), (-0.40, 0.12),
            (-0.27, 0.06), (-0.28, -0.08), (-0.54, -0.16), (-0.90, -0.14)
        ], 0.12, wood)
        add_cube(root, col, "Wood foregrip", (0.16, 0.0, -0.045), (0.28, 0.12, 0.12), wood, 0.028)
        add_cylinder(root, col, "Barrel", (0.66, 0.0, 0.035), 0.028, 0.78, steel)
        add_cube(root, col, "Front compensator", (1.02, 0.0, 0.035), (0.12, 0.075, 0.075), metal, 0.01)
        for i in range(5):
            add_cylinder(root, col, "Cooling jacket ring %02d" % i, (0.27 + i * 0.055, 0.0, 0.035), 0.069, 0.018, steel, axis="X", vertices=24)
        add_cylinder(root, col, "Drum magazine silhouette", (0.10, 0.0, -0.18), 0.14, 0.075, metal, axis="Y", vertices=32)
        add_cube(root, col, "Pistol grip", (-0.10, 0.0, -0.19), (0.12, 0.10, 0.27), wood, 0.025, (0.0, -0.17, 0.0))
        add_cube(root, col, "Rear sight", (-0.29, 0.0, 0.13), (0.08, 0.065, 0.07), steel, 0.006)
    elif kind == "mp40":
        add_cube(root, col, "Upper receiver", (-0.01, 0.0, 0.045), (0.56, 0.12, 0.12), metal, 0.025)
        add_cylinder(root, col, "Barrel", (0.55, 0.0, 0.045), 0.022, 0.55, steel)
        add_cube(root, col, "Front shroud", (0.42, 0.0, 0.045), (0.27, 0.085, 0.09), metal, 0.015)
        add_cube(root, col, "Stick magazine silhouette", (0.02, -0.005, -0.19), (0.13, 0.085, 0.33), metal, 0.012, (0.0, -0.05, 0.0))
        add_cube(root, col, "Magazine well", (0.02, 0.0, -0.065), (0.18, 0.10, 0.08), steel, 0.01)
        add_cube(root, col, "Pistol grip", (-0.23, 0.0, -0.19), (0.12, 0.09, 0.26), rubber, 0.022, (0.0, -0.18, 0.0))
        add_cube(root, col, "Folding stock upper", (-0.59, 0.0, 0.08), (0.48, 0.045, 0.035), steel, 0.012)
        add_cube(root, col, "Folding stock lower", (-0.72, 0.0, -0.01), (0.08, 0.045, 0.20), steel, 0.012)
        add_cube(root, col, "Rear sight", (-0.30, 0.0, 0.13), (0.075, 0.065, 0.06), steel, 0.005)
    else:
        add_cylinder(root, col, "Tubular receiver", (0.0, 0.0, 0.035), 0.065, 0.76, metal, axis="X", vertices=24)
        add_cylinder(root, col, "Barrel", (0.65, 0.0, 0.035), 0.025, 0.62, steel)
        add_cylinder(root, col, "Muzzle collar", (0.94, 0.0, 0.035), 0.044, 0.08, metal)
        add_cube(root, col, "Side magazine silhouette", (0.04, -0.13, -0.16), (0.095, 0.28, 0.30), metal, 0.012)
        add_cube(root, col, "Simple shoulder stock", (-0.65, 0.0, 0.02), (0.36, 0.045, 0.045), steel, 0.008)
        add_cube(root, col, "Front handguard", (0.29, 0.0, -0.08), (0.25, 0.09, 0.08), wood, 0.014)
        add_cube(root, col, "Pistol grip", (-0.14, 0.0, -0.17), (0.11, 0.09, 0.22), metal, 0.018, (0.0, -0.18, 0.0))
    add_cube(root, col, "Trigger guard", (-0.22, 0.0, -0.12), (0.22, 0.04, 0.03), steel, 0.01)
    add_cube(root, col, "Front sight", (0.93 if kind != "sten" else 0.92, 0.0, 0.105), (0.035, 0.04, 0.055), steel, 0.004)
    return root

def add_bren(position):
    root, col = new_weapon("Bren Mk II - provisional", position, 0)
    add_profile(root, col, "Wood buttstock", [
        (-1.05, -0.12), (-1.0, 0.13), (-0.48, 0.12),
        (-0.24, 0.08), (-0.22, -0.12), (-0.48, -0.2), (-0.9, -0.17)
    ], 0.14, wood)
    add_cube(root, col, "Receiver", (0.0, 0.0, 0.02), (0.62, 0.14, 0.17), metal, 0.025)
    add_cylinder(root, col, "Barrel", (0.77, 0.0, 0.05), 0.028, 1.15, steel)
    add_cylinder(root, col, "Barrel jacket", (0.47, 0.0, 0.05), 0.065, 0.50, metal, axis="X")
    add_cube(root, col, "Wood handguard", (0.39, 0.0, -0.045), (0.50, 0.12, 0.11), wood, 0.02)
    add_cube(root, col, "Top curved magazine base", (0.20, 0.0, 0.16), (0.35, 0.14, 0.08), metal, 0.015)
    for i in range(4):
        add_cube(root, col, "Curved magazine segment %02d" % i, (0.05 + i * 0.075, 0.0, 0.23 + math.sin((i + 1) * 0.42) * 0.10), (0.12, 0.14, 0.09), metal, 0.012, (0.0, -0.14, 0.0))
    add_cube(root, col, "Rear sight", (-0.26, 0.0, 0.16), (0.08, 0.07, 0.09), steel, 0.006)
    add_cube(root, col, "Front sight", (1.25, 0.0, 0.14), (0.04, 0.05, 0.08), steel, 0.005)
    add_cube(root, col, "Pistol grip", (-0.15, 0.0, -0.20), (0.13, 0.11, 0.28), wood, 0.025, (0.0, -0.16, 0.0))
    add_cube(root, col, "Bipod left leg", (0.49, -0.20, -0.28), (0.045, 0.04, 0.42), steel, 0.01, (0.0, -0.18, -0.12))
    add_cube(root, col, "Bipod right leg", (0.49, 0.20, -0.28), (0.045, 0.04, 0.42), steel, 0.01, (0.0, -0.18, 0.12))
    add_cube(root, col, "Trigger guard", (-0.25, 0.0, -0.13), (0.22, 0.04, 0.03), steel, 0.01)
    return root

def add_pistol(position):
    root, col = new_weapon("Colt M1911A1 - provisional", position, 0)
    add_cube(root, col, "Steel frame", (0.0, 0.0, 0.02), (0.43, 0.11, 0.14), metal, 0.025)
    add_cube(root, col, "Slide", (0.09, 0.0, 0.105), (0.45, 0.105, 0.075), steel, 0.018)
    add_cylinder(root, col, "Short barrel", (0.34, 0.0, 0.11), 0.023, 0.17, metal)
    add_cube(root, col, "Front sight", (0.29, 0.0, 0.155), (0.035, 0.03, 0.035), steel, 0.003)
    add_cube(root, col, "Rear sight", (-0.14, 0.0, 0.155), (0.045, 0.04, 0.035), steel, 0.003)
    add_cube(root, col, "Grip panels", (-0.13, 0.0, -0.12), (0.12, 0.13, 0.25), wood, 0.018, (0.0, -0.22, 0.0))
    add_cube(root, col, "Magazine silhouette", (-0.02, 0.0, -0.17), (0.08, 0.07, 0.12), steel, 0.01)
    add_cube(root, col, "Trigger guard", (0.04, 0.0, -0.10), (0.18, 0.045, 0.03), steel, 0.01)
    add_cube(root, col, "Trigger", (0.055, 0.0, -0.07), (0.025, 0.02, 0.06), steel, 0.003)
    return root

weapons = [
    ("Mauser 1909 argentino - provisional", long_rifle, "mauser"),
    ("M1 Garand - provisional", long_rifle, "garand"),
    ("Karabiner 98k - provisional", long_rifle, "kar98"),
    ("Thompson M1928 - provisional", add_submachine_gun, "thompson"),
    ("MP 40 - provisional", add_submachine_gun, "mp40"),
    ("Sten Mk II - provisional", add_submachine_gun, "sten"),
]
for index, (label, builder, variant) in enumerate(weapons):
    position = ((index % 3) * 3.2 - 3.2, (index // 3) * 2.4 - 0.6, 0.0)
    if builder is long_rifle:
        builder(label, position, variant)
    else:
        builder(label, position, variant)

add_bren((6.4, -0.6, 0.0))
add_pistol((-6.4, -0.6, 0.0))

# Presentation camera and neutral studio lighting for a useful first preview.
camera_data = bpy.data.cameras.new("Arsenal Preview Camera")
camera = bpy.data.objects.new("Arsenal Preview Camera", camera_data)
scene.collection.objects.link(camera)
camera.location = (0.0, -24.0, 7.2)
camera.rotation_euler = (Vector((0.0, 0.0, 0.08)) - camera.location).to_track_quat("-Z", "Y").to_euler()
camera.data.type = "ORTHO"
camera.data.ortho_scale = 19.0
scene.camera = camera

def add_area(name, location, energy, size):
    light_data = bpy.data.lights.new(name, "AREA")
    light_data.energy = energy
    light_data.shape = "DISK"
    light_data.size = size
    light = bpy.data.objects.new(name, light_data)
    scene.collection.objects.link(light)
    light.location = location
    direction = Vector((0.0, 0.0, 0.2)) - light.location
    light.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()

add_area("Key Light", (0.0, -8.0, 10.0), 1800.0, 9.0)
add_area("Fill Light", (-8.0, -4.0, 5.0), 900.0, 8.0)
add_area("Rim Light", (7.0, 1.0, 8.0), 1300.0, 7.0)
scene.render.filepath = RENDER_PATH
scene.view_settings.view_transform = "AgX"
scene.render.image_settings.color_depth = "8"

bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
scene.render.engine = "BLENDER_WORKBENCH"
scene.display.shading.color_type = "MATERIAL"
scene.display.shading.light = "STUDIO"
scene.display.shading.studio_light = "paint.sl"
scene.display.shading.background_type = "WORLD"
scene.display.shading.show_shadows = True
scene.display.shading.show_cavity = True
scene.display.shading.cavity_type = "BOTH"
bpy.ops.render.render(write_still=True)
scene.render.engine = "BLENDER_EEVEE"
bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)
