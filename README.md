# AzFunctionWizard

Sample solution that demonstrates the following:

1. A VSIX deployable Project Template, that creates a custom Azure Function project
2. An associated wizard ui, implemented with WPF that matches the selected VS theme.

The AxFunctionTemplate project, is the project template and represents the files that will become the resulting Azure Function project.
The AzFunctionWizard project, is the project that generates the AzFunctionWizard.dll, and .VSIX file that can be used to install the template.

Because the project file (MyFunctionApp.csproj) is an SDK style project, the associated wizard assembly, must programmatically copy and detokenize the template files, because the VS wizard engine only processes files explicitly included in the target .csproj file. Review the ProcessVSTemplate method in the Wizard.cs file for the details.
