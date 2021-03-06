﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Content;
using tso.world.components;
using tso.world.model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSO.Files.formats.iff.chunks;
using TSO.Simantics.model;

namespace TSO.Simantics
{
    public class VMGameObject : VMEntity
    {

        private VMEntity[] SlotContainees;

        /** Definition **/

        public VMGameObject(GameObject def, ObjectComponent worldUI) : base(def)
        {
            this.WorldUI = worldUI;

            /*var mainFunction = def.Master.MainFuncID;
            if (mainFunction > 0)
            {
                var bhav = def.Iff.BHAVs.First(x => x.ID == mainFunction);
                int y = 22;
            }*/
        }

        public override void SetDynamicSpriteFlag(ushort index, bool set)
        {
            base.SetDynamicSpriteFlag(index, set);
            if (this.WorldUI != null){
                ((ObjectComponent)this.WorldUI).DynamicSpriteFlags = this.DynamicSpriteFlags;
            }
        }

        public override bool SetValue(VMStackObjectVariable var, short value)
        {
            return base.SetValue(var, value);
        }

        public override short GetValue(VMStackObjectVariable var)
        {
            return base.GetValue(var);
        }

        public bool RefreshGraphic()
        {
            var newGraphic = Object.OBJ.BaseGraphicID + ObjectData[(int)VMStackObjectVariable.Graphic];
            var dgrp = Object.Resource.Get<DGRP>((ushort)newGraphic);
            if (dgrp != null)
            {
                ((ObjectComponent)WorldUI).DGRP = dgrp;
                return true;
            }
            return false;
        }


        public override void Init(TSO.Simantics.VMContext context){
            ((ObjectComponent)WorldUI).ObjectID = ObjectID;
            if (Slots != null && Slots.Slots.ContainsKey(0))
            {
                SlotContainees = new VMEntity[Slots.Slots[0].Count];
                ((ObjectComponent)WorldUI).ContainerSlots = Slots.Slots[0];
            }
            base.Init(context);
            //context.World.AddComponent(this.WorldUI);

            /** Aquarium, we will allow the load and main functions to run for this object **/
  
        }

        public override Direction Direction { 
            get { return ((ObjectComponent)WorldUI).Direction; }
            set { ((ObjectComponent)WorldUI).Direction = value; }
        }
        public override Vector3 Position { 
            get { return WorldUI.Position; }
            set { /*yeah should probably implement this*/ }
        }

        public override string ToString()
        {
            var strings = Object.Resource.Get<CTSS>(Object.OBJ.CatalogStringsID);
            if (strings != null){
                return strings.GetString(0);
            }
            var label = Object.OBJ.ChunkLabel;
            if (label != null && label.Length > 0){
                return label;
            }
            return Object.OBJ.GUID.ToString("X");
        }

        // Begin Container SLOTs interface

        public override void PlaceInSlot(VMEntity obj, int slot)
        {
            if (SlotContainees != null)
            {
                if (slot > -1 && slot < SlotContainees.Length)
                {
                    SlotContainees[slot] = obj;
                    obj.SetValue(VMStackObjectVariable.ContainerId, this.ObjectID);
                    obj.SetValue(VMStackObjectVariable.SlotNumber, (short)slot);
                }
            }
        }

        public override VMEntity GetSlot(int slot)
        {
            if (SlotContainees != null)
            {
                if (slot > -1 && slot < SlotContainees.Length)
                {
                    return SlotContainees[slot];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public override void ClearSlot(int slot)
        {
            if (SlotContainees != null)
            {
                if (slot > -1 && slot < SlotContainees.Length)
                {
                    SlotContainees[slot] = null;
                }
            }
        }

        // End Container SLOTs interface

        public override Texture2D GetIcon(GraphicsDevice gd)
        {
            var bmp = Object.Resource.Get<BMP>(Object.OBJ.CatalogStringsID);
            if (bmp != null) return bmp.GetTexture(gd);
            else return null;
        }

    }
}
