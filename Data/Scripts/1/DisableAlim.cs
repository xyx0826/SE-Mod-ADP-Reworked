using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using static Sandbox.ModAPI.Ingame.TerminalBlockExtentions;

namespace DisableModdedBlocks
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Assembler), true, "DrillPlatform")]
    public class DisableBlocks : MyGameLogicComponent
    {
        /// <summary>
        ///     Disable all blocks closer than this number of meters.
        /// </summary>
        private const double DisableDistance = 200;

        private static readonly HashSet<IMyEntity> myModdedBlocks = new HashSet<IMyEntity>();
        private static readonly HashSet<IMyEntity> myClosedBlocks = new HashSet<IMyEntity>();
        private MyObjectBuilder_EntityBase objectBuilder;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            this.objectBuilder = objectBuilder;
            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            myModdedBlocks.Add(Entity);
            base.Init(objectBuilder);
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return copy ? objectBuilder.Clone() as MyObjectBuilder_EntityBase : objectBuilder;
        }

        public override void UpdateAfterSimulation10()
        {
            foreach (var moddedBlock in myModdedBlocks)
            {
                if (moddedBlock.EntityId == Entity.EntityId)
                    continue;
                if (moddedBlock.Closed || moddedBlock.MarkedForClose)
                {
                    myClosedBlocks.Add(moddedBlock);
                    continue;
                }
                    

                var dist = (moddedBlock.GetPosition() - Entity.GetPosition()).LengthSquared();
                if (dist < DisableDistance * DisableDistance)
                    (Entity as IMyTerminalBlock).ApplyAction("OnOff_Off");
            }
            foreach (var moddedBlock in myClosedBlocks)
            {
                myModdedBlocks.Remove(moddedBlock);
            }
        }

        public override void Close()
        {
            myModdedBlocks.Remove(Entity);
            base.Close();
        }
    }
}