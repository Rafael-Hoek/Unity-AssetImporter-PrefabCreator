
------------------------------------
---------- PREFAB CREATOR ----------
------------------------------------

-----INTRODUCTION-----
This is a guide to the prefab creator/asset importer. This tool is meant to aid when importing meshes and textures into 
the correct folders in Unity, and automates the process of creating materials and prefabs based on the imported meshes and 
textures. Read the installation and quick guide below for an overview of the most important features. Read the user guide for an 
in-depth explanation of all the features and constraints of the tool.

author: Rafael Hoek

-----INSTALLATION-----
 - Import this package as you would any other unity package. It should import into the Assets>Project>Scripts>Editor folder

-----QUICK QUIDE-----
This is a quick start guide, that will guide you through a simple example application of the tool. This is not an exhaustive explanation 
of all the features of the tool, but it will give you the gist. After following the installation steps, do the following:
  1. In an empty folder we'll call "Foo", make the following files:
    - FooMesh.fbx (a mesh)
    - FooMesh_Collision.fbx (a collision mesh for the created mesh)
    - FooTexture.tif (an albedo for the created mesh)
    - FooTexture_M.tif (metallic)
    - FooTexture_N.tif (normal)
    - FooTexture_AO.tif (ambient occlusion)
    - FooTexture_Mask.tif (Mask)
  2. In an open unity project, in the toolbar at the top of the screen select 'Project', then 'Asset Tools', then 'Add New Prefab'
  3. In the window that opens, named 'Add Prefab', click on the button 'Select Folder'
  4. In the folder selection menu that opens, navigate to and select the 'Foo'-folder we made earlier
  5. Back in the Add Prefab window, click on the button 'Retrieve Files from Folder'
    - You should see a list of files appear under a header "Files in folder". The names of all the files we created earlier should be
      shown next to the correct category of file (the mesh file next to mesh, the albedo file next to albedo etc.)
  6. In the textbox next to 'Prefab Name', remove the text "FooMesh" and instead write "FooPrefab"
  7. In the dropdown-menu next to 'Prefab Type', select 'Prop'
  8. Click on the button 'Create prefab'
    - After a few loading bars flash on the screen, several buttons beginning with 'Ping' should appear
  9. Open the tab 'Project', which one uses for navigating through menus, and click on the button 'Ping Prefab'. This should highlight 
     the created prefab with the name "FooPrefab"
  10. Also click the button 'Ping Material'. This should ping the material that was created, with the name 'FooTexture'

-----USER GUIDE-----

   ---OPENING THE TOOL---
 - You can find the tool in the unity tool bar (top of screen) under Project>Asset Tools>Add New Prefab. Click on this button
   to open up the Add Prefab tool window

   ---SELECTING FILES TO BE IMPORTED---
 - First, you will have to select which files you wish to import
 - Click on the 'Select Folder'-button, and navigate to the folder that contains the files you wish to import into unity in 
   the file explorer window that pops up
 - Click on 'Select Folder'. You should see the path of the folder you have selected appear in the 'Selected Folder'-textbox
 - There may be files in the folder you have selected that you do not want to import. You can filter out files you do not wish
   to include by entering a search term in the 'Select files with name'-textbox.
    > This will mean only files containing the entered search term will be selected when attempting to import them
 - Click on the 'Retrieve Files from Folder'-button to retrieve all the files from the selected folder that match the entered
   search-term
 - A few new elements will appear on the tool window now. A quick overview:
    > Prefab Name: The name that the eventually created prefab will have, if you choose to create a Prefab
    > Prefab Type: The 'type' of the assets you wish to import. This mostly determines which folder the assets will be supported
      into when importing them
    > Files in folder: below this title are the names of all the files the tool will attempt to import. You may find under this title:
        ~ Mesh: an asset mesh. This may only be a .fbx-file.
        ~ Collision Mesh: a mesh that will be used to create collision objects for the given mesh
            # This must be a .fbx-file
            # This file must be marked by adding a '_Collision'-tag on the end. An example of a valid collision mesh file: 
              "TestMesh01_Collision.fbx"
        ~ Material Textures: Under this subheader are all the different textures that have been detected. These are categorised by 
          name, so that all texture files with the same name (disregarding tag) will be used to create a single Material asset.
            # An example: "TestTex01.tif", "testTex01_M.tif", "testTex01_N.tif" will be used to create a material "testTex01". Another
              file "TestTex02.tif" will also be detected and imported, but will be used to make a separate material; "testTex02"
            # All texture files have to be a .tif, .tiff, .png or .psd format
        ~ Albedo: A texture file. This should not have a tag at the end
        ~ Metallic: A metalic texture file. This should have the '_M'-tag
        ~ Normal: A normal texture file. This should have the '_N'-tag
        ~ Ambient Occlusion (AO): An ambient occlusion texture file. This should have the '_AO'-tag
        ~ Mask: Mask texture file. This should have the '_Mask'-tag

   ---IMPORTING FILES---
 - The use of the tool is of course to eventually import selected assets into your unity project. This can be done in two ways:
    > Importing as raw assets: copying the files into the correct folder within the project, and if the correct files are available, 
      creating materials from the given textures.
        ~ At least an Albedo texture file has to be available for a material to be created. All other files are optional
    > Creating a prefab: This will import all the selected files, as before, but will also use given meshes and textures to create 
      a prefab
        ~ At least a mesh file is required for this. Otherwise the files will be copied without a prefab being created
        ~ If an albedo-file is present, a material will be created and automatically added to the prefab. Automatic addition of materials
          to a prefab is predicated on the condition that only one material has been detected in the manner explained above. If multiple 
          materials have been detected, they will each be generated, but not automatically added to the prefab
 - Either way, to import the assets, one must first select a prefab type. This is because the prefab type will be used in sorting the
   imported assets into the correct folders automatically
 - Two checkboxes are also present in the menu:
    > Overwrite Existing Files: If any duplicate files are present (files with the same name as the file you wish to import), selecting this 
      option will mean these files are replaced. If this option is not selected, the import operation will cease at the point that the duplicate
      file tries to import. Careful; don't use this setting by default. You may overwrite files without realising it if you always keep this on.
    > Delete collision mesh after use: you don't always want a collision mesh to be imported with the rest. It can clutter your mesh storage and 
      is no longer really useful after importing. Selecting this option will delete the imported collision mesh after is has been used to generated
      collider objects.
 - Once a prefab type has been selected, two buttons will appear; Create prefab and Add as raw assets. The former will automatically create
   a prefab, the latter will not, as described above.
 - Once the assets have been imported, a series of buttons will appear labeled "ping ...". These buttons, when pressed, will "ping" the 
   corresponding file within the unity project file structure in the Project-tab
 - Another notable feature is that labels (the same label as the name of the selected prefab type) have automatically been added to the 
   imported and created assets

-----ADDING PREFAB TYPES-----
Adding prefab types is quite simple. The sorting of assets happens automatically. All that has to be specified is the exact folder and 
the name of the prefab type. This can be done in the manner below:
  1. Open the PrefabType.cs-file in any IDE (you can even use notepad if you like)
  2. In the section "public enum PrefabType", where you see NONE, Structure and Prop (and potentially other categories), add a new line
     and write the name of your new category (using capital first letter) and assign it the next available integer value. An example of 
     a new entry is: "Bar = 3,". DO NOT FORGET THE COMMA
  3. Under the section "public static readonly Dictionary<...> List = new Dictionary<...>()" add, under the existing entries, a new line
     from the template below:
       - {PrefabCreator.PrefabType.[PrefabTypeName], "[Path/To/Prefab/Folder]"},
       - Replace the square brackets and its contents with the following:
          > PrefabTypeName is the name of the new prefabtype. Make sure this is the same name as you entered in the previous step in the enum
          > Path/To/Prefab/Folder is the folder in which the prefabs should be stored. Enter the path starting from but not including the 
            asset type
  4. Save and close the file. Unity will then recompile, and your changes will have been saved, and the new prefab type will be available to 
     select