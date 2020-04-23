# LogicReinc.WindowsSandbox
Not long ago Microsoft released Windows Sandbox into the pro license of Windows 10, offering a small VM that starts empty every time from just creating a .wsb file.

While a great idea, it lacks in easily creating any kind of persistence. So you will either have to write startup scripts that setup your environment or install them each time you open the VM. 

This wrapper allows you to create slightly different config files in json, as well as config files to describe application/tool installation (a little like Docker).

You can create ComponentDescriptors, which tell the wrapper how to setup a specific app. After that you can create .sandbox files everywhere that allow you to reference these components by just a simple ID.

After creating a .sandbox file you can double click it if you registered the wrapper application (run as administrator to start register process).

## Sandbox  Example (.sandbox)
The following would setup a sandbox with Chrome and Winrar installed (assuming you have defined Chrome and Winrar).
```json
 {
  "ID": "example",
  "Components":[
    "Chrome",
    "Winrar"
  ]
}
```
Saving this as a .sandbox file will allow you to simply open it as a sandbox.

## Component Example (.json)
Putting the following .json in the "Components" directory where the wrapper is located will cause it to be available under the given ID.
```json
{
  "ID": "Winrar",
  "Reference": {
    "winrar":  "Installation/winrar.exe"
  },
  "Startup": [
    "start /wait {winrar} /s"
  ]
}
```
"References" are files and directory added as read-only. And their path will be available using {winrar} in this example.
Another example could be:
```json
{
  "ID": "Brave",
  "Preserve": [
	"{AppData}/Local/BraveSoftware/Brave-Browser/User Data"
  ],
  "Data": {
    "bravePath": "Installations/Brave"
  },
  "Shortcuts": [
    "{bravePath}/Brave-Browser/Application/brave.exe"
  ]
}
```
Unlike "References", "Data" is a copy of the original. Data is write-able and is copied whenever the data is missing. The wrapper will group data by the given sandbox ID. Changing the ID or deleting the sandbox's data directory will cause it to copy the data again. 
"Preserve" will cause a given path to be preserved after closing. This is done by creating a symlink to the sandbox's data directory.