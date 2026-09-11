using Emby.Web.GenericEdit;
using Emby.Web.GenericEdit.Elements;
using Emby.Web.GenericEdit.Elements.List;
using Emby.Web.GenericEdit.Validation;
using MediaBrowser.Model.Attributes;
using MediaBrowser.Model.LocalizationAttributes;
using StrmAssistant.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using static StrmAssistant.Options.GeneralOptions;

namespace StrmAssistant.Options
{
    public class PluginOptions : EditableOptionsBase
    {
        public override string EditorTitle => global::StrmAssistant.Plugin.DisplayName;

        public override string EditorDescription => string.Empty;
        
        public GenericItemList Disclaimer { get; set; } = new GenericItemList();

        [VisibleCondition(nameof(ShowConflictPluginLoadedStatus), SimpleCondition.IsTrue)]
        public StatusItem ConflictPluginLoadedStatus { get; set; } = new StatusItem();

        [VisibleCondition(nameof(IsModSuccess), SimpleCondition.IsFalse)]
        public StatusItem ModStatus { get; set; } = new StatusItem();

        [DisplayNameL("GeneralOptions_EditorTitle_General_Options", typeof(Resources))]
        public GeneralOptions GeneralOptions { get; set; } = new GeneralOptions();

        [DisplayNameL("PluginOptions_EditorTitle_Strm_Extract", typeof(Resources))]
        public MediaInfoExtractOptions MediaInfoExtractOptions { get; set; } = new MediaInfoExtractOptions();
        
        [DisplayNameL("PluginOptions_MetadataEnhanceOptions_Metadata_Enhance", typeof(Resources))]
        public MetadataEnhanceOptions MetadataEnhanceOptions { get; set; } = new MetadataEnhanceOptions();

        [DisplayNameL("PluginOptions_IntroSkipOptions_Intro_Credits_Detection", typeof(Resources))]
        public IntroSkipOptions IntroSkipOptions { get; set; } = new IntroSkipOptions();

        [DisplayNameL("ExperienceEnhanceOptions_EditorTitle_Experience_Enhance", typeof(Resources))]
        public ExperienceEnhanceOptions ExperienceEnhanceOptions { get; set; } = new ExperienceEnhanceOptions();

        [DisplayNameL("AboutOptions_EditorTitle_About", typeof(Resources))]
        public AboutOptions AboutOptions { get; set; } = new AboutOptions();

        [Browsable(false)]
        public bool? IsModSuccess => true;

        [Browsable(false)]
        public bool ShowConflictPluginLoadedStatus =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetName().Name)
                .Any(n => n == "StrmExtract" || n == "InfuseSync");

        protected override void Validate(ValidationContext context)
        {
            if (GeneralOptions.CatchupMode &&
                GeneralOptions.CatchupTaskScope.Contains(CatchupTask.Fingerprint.ToString()) &&
                !IntroSkipOptions.UnlockIntroSkip)
            {
                context.AddValidationError(Resources.InvalidFingerprintCatchup);
            }
        }
    }
}
