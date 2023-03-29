using p3ppc.outfitSelector.Template.Configuration;
using System.ComponentModel;
using static p3ppc.outfitSelector.Enums;

namespace p3ppc.outfitSelector.Configuration
{
    public class Config : Configurable<Config>
    {
        /*
            User Properties:
                - Please put all of your configurable properties here.

            By default, configuration saves as "Config.json" in mod user config folder.    
            Need more config files/classes? See Configuration.cs

            Available Attributes:
            - Category
            - DisplayName
            - Description
            - DefaultValue

            // Technically Supported but not Useful
            - Browsable
            - Localizable

            The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
        */

        [DisplayName("Male Protag Outfit")]
        [Description("The outfit the male protagonist should wear")]
        [DefaultValue(MaleProtagOutfit.Default)]
        public MaleProtagOutfit MaleProtagOutfit { get; set; } = MaleProtagOutfit.Default;

        [DisplayName("Female Protag Outfit")]
        [Description("The outfit the female protagonist should wear")]
        [DefaultValue(FemaleProtagOutfit.Default)]
        public FemaleProtagOutfit FealeProtagOutfit { get; set; } = FemaleProtagOutfit.Default;

        [DisplayName("Yukari Outfit")]
        [Description("The outfit Yukari should wear")]
        [DefaultValue(YukariOutfit.Default)]
        public YukariOutfit YukariOutfit { get; set; } = YukariOutfit.Default;

        [DisplayName("Aigis Outfit")]
        [Description("The outfit Aigis should wear")]
        [DefaultValue(AigisOutfit.Default)]
        public AigisOutfit AigisOutfit { get; set; } = AigisOutfit.Default;

        [DisplayName("Mitsuru Outfit")]
        [Description("The outfit Mitsuru should wear")]
        [DefaultValue(MitsuruOutfit.Default)]
        public MitsuruOutfit MitsuruOutfit { get; set; } = MitsuruOutfit.Default;

        [DisplayName("Junpei Outfit")]
        [Description("The outfit Junpei should wear")]
        [DefaultValue(JunpeiOutfit.Default)]
        public JunpeiOutfit JunpeiOutfit { get; set; } = JunpeiOutfit.Default;

        [DisplayName("Akihiko Outfit")]
        [Description("The outfit Akihiko should wear")]
        [DefaultValue(AkihikoOutfit.Default)]
        public AkihikoOutfit AkihikoOutfit { get; set; } = AkihikoOutfit.Default;

        [DisplayName("Ken Outfit")]
        [Description("The outfit Ken should wear")]
        [DefaultValue(KenOutfit.Default)]
        public KenOutfit KenOutfit { get; set; } = KenOutfit.Default;

        [DisplayName("Shinjiro Outfit")]
        [Description("The outfit Shinjiro should wear")]
        [DefaultValue(ShinjiroOutfit.Default)]
        public ShinjiroOutfit ShinjiroOutfit { get; set; } = ShinjiroOutfit.Default;

        [DisplayName("Koromaru Outfit")]
        [Description("The outfit Koromaru should wear")]
        [DefaultValue(KoromaruOutfit.Default)]
        public KoromaruOutfit KoromaruOutfit { get; set; } = KoromaruOutfit.Default;

        [DisplayName("Debug Mode")]
        [Description("Logs additional information to the console that is useful for debugging.")]
        [DefaultValue(false)]
        public bool DebugEnabled { get; set; } = false;
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}