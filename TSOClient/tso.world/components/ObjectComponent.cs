﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using TSO.Content;
using tso.world.utils;
using TSO.Files.formats.iff.chunks;
using tso.world.model;
using Microsoft.Xna.Framework;

namespace tso.world.components
{
    public class ObjectComponent : WorldComponent
    {
        private GameObject Obj;
        private DGRP DrawGroup;
        private DGRPRenderer dgrp;
        public WorldObjectRenderInfo renderInfo;
        public Blueprint blueprint;
        public List<SLOTItem> ContainerSlots;
        public short ObjectID; //set this any time it changes so that hit test works.

        public override Vector3 GetSLOTPosition(int slot)
        {
            return this.Position; //todo: add position of slot
        }

        public ObjectComponent(GameObject obj){
            this.Obj = obj;
            renderInfo = new WorldObjectRenderInfo();
            if (obj.OBJ.BaseGraphicID > 0)
            {
                var gid = obj.OBJ.BaseGraphicID;
                //if (obj.OBJ.GUID == 0x98E0F8BD)
                //{
                //    var dgroups = obj.Resource.List<DGRP>();
                //    gid += 10;
                //    gid = 125;
                //}
                this.DrawGroup = obj.Resource.Get<DGRP>(gid);
                dgrp = new DGRPRenderer(this.DrawGroup);
                dgrp.DynamicSpriteBaseID = obj.OBJ.DynamicSpriteBaseId;
                dgrp.NumDynamicSprites = obj.OBJ.NumDynamicSprites;
            }
        }

        public DGRP DGRP
        {
            get
            {
                return DrawGroup;
            }
            set
            {
                DrawGroup = value;
                dgrp.DGRP = value;
                blueprint.Damage.Add(new BlueprintDamage(BlueprintDamageType.SCROLL, TileX, TileY, Level));
            }
        }

        private ushort _DynamicSpriteFlags = 0x0000;
        public ushort DynamicSpriteFlags
        {
            get{
                return _DynamicSpriteFlags;
            }set{
                _DynamicSpriteFlags = value;
                if (dgrp != null){
                    dgrp.DynamicSpriteFlags = value;
                }
            }
        }

        public override float PreferredDrawOrder
        {
            get {
                return 2000.0f + (this.Position.X + this.Position.Y);
            }
        }

        private Direction _Direction;
        public Direction Direction
        {
            get
            {
                return _Direction;
            }
            set
            {
                _Direction = value;
                if (dgrp != null){
                    dgrp.Direction = value;
                    dgrp.InvalidateRotation();
                }
            }
        }

        public override void OnRotationChanged(WorldState world)
        {
            base.OnRotationChanged(world);
            if (dgrp != null){
                dgrp.InvalidateRotation();
            }
        }

        public override void OnZoomChanged(WorldState world)
        {
            base.OnZoomChanged(world);
            if (dgrp != null){
                dgrp.InvalidateZoom();
            }
        }

        public override void OnScrollChanged(WorldState world)
        {
            base.OnScrollChanged(world);
            if (dgrp != null){
                dgrp.InvalidateScroll();
            }
        }

        //public override void OnPositionChanged(){
        //    base.OnPositionChanged();
        //    if (dgrp != null){
        //        dgrp.Position = this.Position;
        //    }
        //}

        public override void Draw(GraphicsDevice device, WorldState world){
            if (this.DrawGroup == null) { return; }
            //world._2D.Draw(this.DrawGroup);
            dgrp.Draw(world);
        }
    }
}
