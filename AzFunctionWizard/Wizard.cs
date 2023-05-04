using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.TemplateWizard;

namespace AzFunctionWizard
{
    internal class Wizard : IWizard
    {
        private DTE2 _dte;

        public void BeforeOpeningFile(ProjectItem projectItem) { }

        public void ProjectFinishedGenerating(Project project) { }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) { }

        public void RunFinished() { }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            _dte = automationObject as DTE2;

            WizardDialog dlg = new WizardDialog();
            if (dlg.ShowModal() ?? false)
            {
                // todo: add additional dictionary items as needed
                replacementsDictionary.Add("$hellomessage$", dlg.tbMessage.Text);

                // Note, because we're creating a dotnet framework sdk style project, none of the files are explicitly listed in the .csproj.
                // Consequently, the default VS wizard engine will not do token replacement, nor copy the files from the template location,
                // to the project directory.
                //
                // To work around this, we need to copy the files using the location of the .vstemplate file, and the $destinationdirectory$
                ProcessVSTemplate(replacementsDictionary, (string)customParams[0]);
            }
            else
            {
                throw new WizardCancelledException();
            }
        }

        public bool ShouldAddProjectItem(string filePath) { return true; }

        private void ProcessVSTemplate(Dictionary<string, string> replacementsDictionary, string vsTemplateFile)
        {
            string vstemplateDir = Path.GetDirectoryName(vsTemplateFile);
            string destPath = replacementsDictionary["$destinationdirectory$"];

            var xmlDoc = XDocument.Load(vsTemplateFile);
            var ns = xmlDoc.Root.GetDefaultNamespace();

            // find all the projectitem nodes in the .vstemplate file and copy them to the source directory
            foreach (var elem in xmlDoc.Descendants(ns + "ProjectItem"))
            {
                var srcFile = Path.Combine(vstemplateDir, elem.Value);
                var destFile = Path.Combine(destPath, elem.Value);
                var destDir = Path.GetDirectoryName(destFile);

                // make sure subfolders (like the Properties folder) get created.
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                // if there is a ReplaceParameters attribute on the node, and it's set to true, detokenize the file
                if (string.Compare((string)elem.Attribute("ReplaceParameters"), "true", true) == 0)
                {
                    // detokenized and copy file to DestDir
                    DetokenizeAndCopy(replacementsDictionary, srcFile, destFile);
                }
                else
                {
                    // just copy the file as is
                    File.Copy(srcFile, destFile);
                }
            }
        }

        private void DetokenizeAndCopy(Dictionary<string, string> replacementsDictionary, string srcFile, string destFile)
        {
            // suggestion: rewrite to use regex to swap in token values
            string tokenizedSource = File.ReadAllText(srcFile);
            foreach (string key in replacementsDictionary.Keys)
            {
                tokenizedSource = tokenizedSource.Replace(key, replacementsDictionary[key]);
            }
            File.WriteAllText(destFile, tokenizedSource);
        }
    }
}
