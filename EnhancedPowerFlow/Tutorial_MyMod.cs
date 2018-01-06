using System;
using System.Linq;
using System.Collections.Generic;
using FortressCraft.ModFoundation.Block;
using FortressCraft.ModFoundation.Multiblock;

namespace GeniusIsme
{
public class EnhancedPowerFlow : FortressCraft.ModFoundation.Mod
{
    public static Materials InductionCharger;
    public static Materials BatteryMk4;
    public static Materials BatteryMk5;
    public static Materials Conduit;
    public static Materials SolarMk2;
    public static Materials OrganicSolarMk2;

    public override String Name()
    {
        return "GeniusIsme.EnhancedPowerFlow v 0.1.0";
    }

    public override void RegisterEntities()
    {
        this.RegisterBlock("GeniusIsme.EnhancedPowerStorageBlock", p => new PowerStorageBlock(p));
        InductionCharger = this.RegisterBoxMultiblock(
            "GeniusIsme.InductionCharger",
            new Position(5, 1, 5),
            p => new InductionCharger(p),
            p => new PowerStorageFiller<InductionCharger>(p)
        );
        BatteryMk5 = this.RegisterBoxMultiblock(
            "GeniusIsme.BatteryMK5",
            new Position(5, 9, 5),
            p => new BatteryMk5(p),
            p => new PowerStorageFiller<BatteryMk5>(p)
        );
        BatteryMk4 = this.RegisterBoxMultiblock(
            "GeniusIsme.BatteryMK4",
            new Position(3, 3, 3),
            p => new BatteryMk4(p),
            p => new PowerStorageFiller<BatteryMk4>(p)
        );
        Conduit = this.RegisterMultiblock(
            "GeniusIsme.Conduit",
            new List<string> {"Placement"},
            p => new Conduit(p),
            p=> new PowerStorageFiller<Conduit>(p),
            new ConduitBuilder()
        );
        SolarMk2 = this.RegisterBoxMultiblock(
            "GeniusIsme.SolarMk2",
            new Position(3, 1, 3),
            p => new SolarMk2(p),
            p => new Filler<SolarMk2>(p)
        );
        OrganicSolarMk2 = this.RegisterBoxMultiblock(
            "GeniusIsme.OrganicSolarMk2",
            new Position(3, 1, 3),
            p => new OrganicSolarMk2(p),
            p => new Filler<OrganicSolarMk2>(p)
        );
    }
}
}