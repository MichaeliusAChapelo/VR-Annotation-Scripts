## License

This software has no license. The hosting of this project at GitHub allows you to view this repository, however you retain no permission from the creators of this software to use, modify or share this software for any intent or purpose.

This software is under exclusive copyright. You are not allowed to copy, distribute or modify this work without risk of take-downs, litigations or other legal actions.

More info at: https://choosealicense.com/no-permission/#for-users

<!--
# AnnotationInVirtualReality
-->

# Scripts for Annotation in Virtual Reality
This repository contains the C# code for a virtual reality prototype in Unity.


* `AnnotationController.cs`: A Unity component with all code related to the annotation object.
* `DataFormatter.cs`: An independent console program. Takes any number of exported .csv data from a folder and formats them into one .csv to ready for hypothesis testing in R.
* `DataHandler.cs`: Importing and exporting data sets to and from Unity. Converts annotation dots in Unity into .csv spreadsheet files and vice versa.
* `InputMapping.cs`: Simplifies using the touch controller's buttons and maps them to functionality related to the prototype.
* `OVRCustomGrabbable.cs`: A Unity component overriding functionality regarding grabbing objects. Use instead of the `OVRGrabbable` component.

## Images

![ok](https://i.imgur.com/FLL3erQ.png)
![ok](https://i.imgur.com/xBSRATR.png)
![ok](https://i.imgur.com/FlKwBuQ.png)
![ok](https://i.imgur.com/so6T98d.png)
![ok](https://i.imgur.com/xFb8AEi.png)



