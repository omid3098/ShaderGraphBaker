# ShaderGraphBaker
Use ShaderGraph as a texture creation tool!

![image](https://user-images.githubusercontent.com/6388730/178012609-148d4565-ac54-4101-969c-ef7fcfa05011.png)


Left: Procedural brick shader. </br>
Right: Baked shader into 1024 textures.

![image](https://user-images.githubusercontent.com/6388730/177318330-a83377f0-8646-452d-b5d5-7d7ae8cbe1df.png)

Another example:

![Procedural Bath Tile](https://user-images.githubusercontent.com/6388730/178087019-9c886a48-6b54-40ec-b585-4163076bcc0b.gif)


Here is the article on [Medium](https://medium.com/@omid3098/using-unity-s-shadergraph-as-a-procedural-texture-creation-tool-54fc5836534e)

## Package dependencies
Add to manifest.json in your unity project following packages:
- "com.unity.nuget.newtonsoft-json": "3.0.2",
- "com.dbrizov.naughtyattributes": "https://github.com/dbrizov/NaughtyAttributes.git#upm"

## How to contribute
- I am trying to develop more custom nodes to be able to generate more complex procedural textures. feel free to add yours and send a PR.
- Make a better custom window or implement sub-menus directly in ShaderGraph! 
