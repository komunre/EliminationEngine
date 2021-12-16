﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.GameObjects
{
    public class GameObject
    {
        protected Quaternion rotation { get; private set; } = Quaternion.Identity;
        protected Dictionary<Type, EntityComponent> Components { get; private set; } = new();

        public GameObject()
        {
            // AddComponent<Mesh>(); // Add mesh for future usage during drawing
        }

        public void AddComponent<CompType>() where CompType : EntityComponent
        {
            Components.Add(typeof(CompType), Activator.CreateInstance<CompType>());
        }

        public CompType? GetComponent<CompType>() where CompType : EntityComponent
        {
            return Components[typeof(CompType)] as CompType;
        }

        public bool TryGetComponent<CompType>(out CompType? component) where CompType : EntityComponent
        {
            if (Components.TryGetValue(typeof(CompType), out var comp))
            {
                component = comp as CompType;
                return true;
            }
            component = null;
            return false;
        }
    }
}
