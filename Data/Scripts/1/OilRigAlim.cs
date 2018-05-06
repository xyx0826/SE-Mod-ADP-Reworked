using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace P3DResourceRig
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Assembler), true, "DrillPlatform")]
    public class Rig : MyGameLogicComponent
    {
        // Builder is nessassary for GetObjectBuilder method as far as I know.
        private MyObjectBuilder_EntityBase builder;
        private IMyAssembler m_generator;
        private IMyCubeBlock m_parent;

        #region IsInVoxel definition
        private bool IsInVoxel(IMyTerminalBlock block)
        {
            BoundingBoxD blockWorldAABB = block.PositionComp.WorldAABB; // Axis-Aligned Bounding Box of the block
            List<MyVoxelBase> voxelList = new List<MyVoxelBase>();

            MyGamePruningStructure.GetAllVoxelMapsInBox(ref blockWorldAABB, voxelList); // Get all voxels in the block's AABB
            var cubeSize = block.CubeGrid.GridSize;
            BoundingBoxD localAAABB = new BoundingBoxD(cubeSize * ((Vector3D)block.Min - 1), cubeSize * ((Vector3D)block.Max + 1));
            var gridWorldMatrix = block.CubeGrid.WorldMatrix;
            foreach (var map in voxelList)
            {
                if (map.IsAnyAabbCornerInside(ref gridWorldMatrix, localAAABB))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
        #region colors
        private Color m_primaryColor = Color.OrangeRed;
        private Color m_secondaryColor = Color.LemonChiffon;
        public Color PrimaryBeamColor
        {
            get { return m_primaryColor; }
            set
            {
                m_primaryColor = value;               
            }
        }

        public Color SecondaryBeamColor
        {
            get { return m_secondaryColor; }
            set
            {
                m_secondaryColor = value;        
            }
        }
        #endregion
        IMyTerminalBlock terminalBlock;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            m_generator = Entity as IMyAssembler;
            m_parent = Entity as IMyCubeBlock;
            builder = objectBuilder;

            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;

            terminalBlock = Entity as IMyTerminalBlock;
        }
        #region UpdateBeforeSimulation
        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            
            if (m_generator.IsWorking)
            {
                if (IsInVoxel(m_generator as Sandbox.ModAPI.IMyTerminalBlock))
                {
                    IMyInventory inventory = ((Sandbox.ModAPI.IMyTerminalBlock)Entity).GetInventory(1) as IMyInventory;
                    VRage.MyFixedPoint amount = (VRage.MyFixedPoint)(2000 * (1 + (0.4 * m_generator.UpgradeValues["Productivity"])));
                    inventory.AddItems(amount, new MyObjectBuilder_Ore() { SubtypeName = "Stone" });
                    terminalBlock.RefreshCustomInfo();
                }
            }
        }
        #endregion
        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return builder;
        }

        private ulong m_counter = 0;

        //draw counter
        public override void UpdateBeforeSimulation()
        {
            m_counter++;
            if (m_generator.IsWorking)
            {
                if (IsInVoxel(m_generator as IMyTerminalBlock))
                {
                    if (MyAPIGateway.Session?.Player == null)
					{
						return;
					}
					else
					{
						DrawBeams();
					}				
                }
            }
        }

        private void DrawBeams()
        {
            var maincolor = PrimaryBeamColor.ToVector4();
            var auxcolor = SecondaryBeamColor.ToVector4();

            MySimpleObjectDraw.DrawLine(m_generator.WorldAABB.Center - (m_generator.WorldMatrix.Down * 2.5), m_generator.WorldAABB.Center + (m_generator.WorldMatrix.Down * 2.5 * 4), VRage.Utils.MyStringId.GetOrCompute("WeaponLaser"), ref auxcolor, 0.33f);
            MySimpleObjectDraw.DrawLine(m_generator.WorldAABB.Center - (m_generator.WorldMatrix.Down * 2.5), m_generator.WorldAABB.Center + (m_generator.WorldMatrix.Down * 2.5 * 4), VRage.Utils.MyStringId.GetOrCompute("WeaponLaser"), ref maincolor, 1.02f);

            // Draw 'pulsing' beam
            if (m_counter % 2 == 0)
            {
                MySimpleObjectDraw.DrawLine(m_generator.WorldAABB.Center - (m_generator.WorldMatrix.Down * 2.5), m_generator.WorldAABB.Center + (m_generator.WorldMatrix.Down * 2.5 * 4), VRage.Utils.MyStringId.GetOrCompute("WeaponLaser"), ref maincolor, 1.12f);
            }        
        }
    }
}