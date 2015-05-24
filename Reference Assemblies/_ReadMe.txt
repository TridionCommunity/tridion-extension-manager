Use this folder to place DLLs and resources referenced from the Extension Manager projects.

For legal reasons, do NOT add Tridion product DLLs to Git. 
Instead, keep a local copy of the file in the folder. 

This folder should contain the following assemblies:

- Ionic.Zip.dll (under source control)
- Tridion.Web.UI.Core.dll (*)
- Tridion.Web.UI.Editors.CME.dll (*)

(*) On a vanilla installation of the Content Manager, these DLLs can be found in the following location: C:\Program Files (x86)\Tridion\web\WebUI\WebRoot\bin
