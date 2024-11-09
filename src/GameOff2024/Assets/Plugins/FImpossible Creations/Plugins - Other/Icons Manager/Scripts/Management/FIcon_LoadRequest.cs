﻿using UnityEngine;

namespace FIMSpace.FIcons
{
    /// <summary>
    /// FM: Class holding info about requested scaled sprite to be generated by texture container
    /// </summary>
    public class FIcon_LoadRequest
    {
        public int Id { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public FilterMode Filter { get; private set; }
        public bool ScaleDown { get; private set; }
        public FIcon_TextureContainer TargetContainer { get; private set; }
        public FIcons_LoadTask Task { get; private set; }
        public FE_RequestState State { get; private set; }

        public FIcon_LoadRequest(int width, int height, FilterMode filter, int id, FIcon_TextureContainer container, FIcons_LoadTask task, bool scaleDownSupport)
        {
            Width = width;
            Height = height;
            Filter = filter;
            Id = id;
            TargetContainer = container;
            Task = task;
            State = FE_RequestState.None;
            ScaleDown = scaleDownSupport;

            // Automatic keeping aspect ratio
            if (Height <= 0) if (TargetContainer.SourceWidth != 0) Height = FIcons_Methods.AspectRatioHeight(TargetContainer.SourceWidth, TargetContainer.SourceHeight, Width);
        }

        /// <summary>
        /// Progressing request to other state
        /// </summary>
        public void SetState(FE_RequestState state)
        {
            State = state;
        }
    }
}