﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpServer.Engine
{
    using ComponentStore = Dictionary<uint, Component>;

    // SUGGESTION: Transform it to service?
    // TODO: make it use data from DB
    internal static class EntityManager
    {
        private static readonly Dictionary<string, ComponentStore> ComponentDb =
            new Dictionary<string, ComponentStore>();

        private static readonly Dictionary<uint, string> EntityHumanReadableNames = new Dictionary<uint, string>();
        private static readonly List<uint> EntityStore = new List<uint>();
        private static uint _lowestUnassignedEntityId = 1;

        public static void AddComponent<T>(uint entity, T component) where T : Component
        {
            ComponentStore store;
            if (!ComponentDb.TryGetValue(typeof (T).Name, out store))
            {
                store = new ComponentStore();
                ComponentDb[typeof (T).Name] = store;
            }
            store[entity] = component;
        }

        public static void AddComponents(uint entity, params Component[] components)
        {
            foreach (var component in components)
            {
                AddComponent(entity, component);
            }
        }

        public static uint CreateEntity(string name = "No description")
        {
            var entity = GenerateNewEntityId();
            EntityStore.Add(entity);
            EntityHumanReadableNames[entity] = name;
            return entity;
        }

        public static void DestroyEntities(params uint[] entities)
        {
            foreach (var entity in entities)
                DestroyEntity(entity);
        }

        // TODO: Make it synchronous
        public static void DestroyEntity(uint entity)
        {
            if (!EntityStore.Contains(entity))
                return;
            EntityStore.Remove(entity);
            foreach (var componentType in ComponentDb)
            {
                ComponentDb[componentType.Key].Remove(entity);
            }
        }

        public static T[] GetAllComponentsOfType<T>() where T : Component
        {
            return ComponentDb[typeof (T).Name]
                .Values
                .Cast<T>()
                .ToArray();
        }

        // WARNING: Use only for debug reasons (slow as fuck)
        public static T[] GetAllComponentsOnEntity<T>(uint entity) where T : Component
        {
            return ComponentDb
                .Where(componentType => componentType.Value.ContainsKey(entity))
                .Select(componentType => componentType.Value[entity])
                .Cast<T>()
                .ToArray();
        }

        public static uint[] GetAllEntitiesPossessingComponent<T>() where T : Component
        {
            return ComponentDb[typeof (T).Name]
                .Keys
                .ToArray();
        }

        public static T GetComponent<T>(uint entity) where T : Component
        {
            ComponentStore store;
            if (!ComponentDb.TryGetValue(typeof (T).Name, out store))
                throw new KeyNotFoundException("GET FAIL: there are no entities with a Component of class: " +
                                               typeof (T).Name);
            if (!store.ContainsKey(entity))
                throw new KeyNotFoundException("GET FAIL: " + NameFor(entity) + "(ID: " + entity +
                                               ") does not possess Component: " + typeof (T).Name);
            return (T) store[entity];
        }

        public static bool HasComponent<T>(uint entity) where T : Component
        {
            ComponentStore store;
            return ComponentDb.TryGetValue(typeof (T).Name, out store) && store.ContainsKey(entity);
        }

        public static string NameFor(uint entity)
        {
            return EntityHumanReadableNames.ContainsKey(entity) ? EntityHumanReadableNames[entity] : "Not defined";
        }

        public static void RemoveComponent<T>(uint entity) where T : Component
        {
            ComponentStore store;
            if (ComponentDb.TryGetValue(typeof (T).Name, out store))
                store.Remove(entity);
        }

        public static void SetEntityName(uint entity, string name)
        {
            EntityHumanReadableNames[entity] = name;
        }

        // SUGGESTION: Think about better implementation
        // TODO: Make it synchronous
        private static uint GenerateNewEntityId()
        {
            if (_lowestUnassignedEntityId < int.MaxValue)
            {
                return _lowestUnassignedEntityId++;
            }
            for (uint i = 1; i < int.MaxValue; i++)
            {
                if (!EntityStore.Contains(i))
                    return i;
            }
            throw new ArgumentOutOfRangeException("ERROR: no available Entity IDs; too many entities!");
        }
    }
}