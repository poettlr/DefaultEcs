﻿using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using DefaultEcs.System;
using DefaultEcs.Threading;

namespace DefaultEcs.Benchmark.DefaultEcs
{
    public static class EntitySetExtension
    {
        public delegate void EntitySetProcess(ReadOnlySpan<Entity> entities);

        public static void ProcessInParallel(this EntitySet set, EntitySetProcess action)
        {
            int entitiesPerCpu = set.Count / Environment.ProcessorCount;

            Enumerable.Range(0, Environment.ProcessorCount).AsParallel().ForAll(
                i => action(i + 1 == Environment.ProcessorCount ? set.GetEntities().Slice(i * entitiesPerCpu) : set.GetEntities().Slice(i * entitiesPerCpu, entitiesPerCpu)));
        }
    }

    [MemoryDiagnoser]
    public class System
    {
        private struct Position
        {
            public float X;
            public float Y;
        }

        private struct Speed
        {
            public float X;
            public float Y;
        }

        private sealed class TestSystem : AEntitySystem<float>
        {
            public TestSystem(World world, IParallelRunner runner)
                : base(world.GetEntities().With<Position>().With<Speed>().AsSet(), runner)
            { }

            protected override void Update(float state, ReadOnlySpan<Entity> entities)
            {
                foreach (ref readonly Entity entity in entities)
                {
                    Speed speed = entity.Get<Speed>();
                    ref Position position = ref entity.Get<Position>();

                    position.X += speed.X * state;
                    position.Y += speed.Y * state;
                }
            }
        }

        [With(typeof(Speed), typeof(Position))]
        private sealed class TestComponentSystem : AEntitySystem<float>
        {
            private readonly World _world;

            public TestComponentSystem(World world, IParallelRunner runner)
                : base(world, runner)
            {
                _world = world;
            }

            protected override void Update(float state, ReadOnlySpan<Entity> entities)
            {
                Components<Speed> speeds = _world.GetComponents<Speed>();
                Components<Position> positions = _world.GetComponents<Position>();

                foreach (ref readonly Entity entity in entities)
                {
                    Speed speed = entity.Get(speeds);
                    ref Position position = ref entity.Get(positions);

                    position.X += speed.X * state;
                    position.Y += speed.Y * state;
                }
            }
        }

        [With(typeof(Speed), typeof(Position))]
        private sealed class TestBufferedSystem : AEntityBufferedSystem<float>
        {
            public TestBufferedSystem(World world)
                : base(world)
            { }

            protected override void Update(float state, ReadOnlySpan<Entity> entities)
            {
                foreach (ref readonly Entity entity in entities)
                {
                    Speed speed = entity.Get<Speed>();
                    ref Position position = ref entity.Get<Position>();

                    position.X += speed.X * state;
                    position.Y += speed.Y * state;
                }
            }
        }

        private sealed class TestSystemTPL : ISystem<float>
        {
            private readonly EntitySet _set;

            public TestSystemTPL(World world)
            {
                _set = world.GetEntities().With<Position>().With<Speed>().AsSet();
            }

            public bool IsEnabled { get; set; } = true;

            public void Update(float state)
            {
                _set.ProcessInParallel(entities =>
                {
                    foreach (ref readonly Entity entity in entities)
                    {
                        Speed speed = entity.Get<Speed>();
                        ref Position position = ref entity.Get<Position>();

                        position.X += speed.X * state;
                        position.Y += speed.Y * state;
                    }
                });
            }

            public void Dispose()
            {
                _set.Dispose();
            }
        }

        private World _world;
        private DefaultParallelRunner _runner;
        private ISystem<float> _systemSingle;
        private ISystem<float> _system;
        private ISystem<float> _prefetchedSystem;
        private ISystem<float> _bufferedSystem;
        private ISystem<float> _systemTPL;

        [Params(100000)]
        public int EntityCount { get; set; }

        [IterationSetup]
        public void Setup()
        {
            _world = new World(EntityCount);
            _runner = new DefaultParallelRunner(Environment.ProcessorCount);
            _systemSingle = new TestSystem(_world, null);
            _system = new TestSystem(_world, _runner);
            _prefetchedSystem = new TestComponentSystem(_world, _runner);
            _bufferedSystem = new TestBufferedSystem(_world);
            _systemTPL = new TestSystemTPL(_world);

            for (int i = 0; i < EntityCount; ++i)
            {
                Entity entity = _world.CreateEntity();
                entity.Set<Position>();
                entity.Set(new Speed { X = 1, Y = 1 });
            }
        }

        [IterationCleanup]
        public void Cleanup()
        {
            _runner.Dispose();
            _world.Dispose();
            _systemSingle.Dispose();
            _system.Dispose();
            _prefetchedSystem.Dispose();
            _bufferedSystem.Dispose();
            _systemTPL.Dispose();
        }

        [Benchmark]
        public void DefaultEcs_UpdateSingle() => _systemSingle.Update(1f / 60f);

        [Benchmark]
        public void DefaultEcs_UpdateMulti() => _system.Update(1f / 60f);

        [Benchmark]
        public void DefaultEcs_UpdateMultiComponent() => _prefetchedSystem.Update(1f / 60f);

        [Benchmark]
        public void DefaultEcs_UpdateBuffered() => _bufferedSystem.Update(1f / 60f);

        [Benchmark]
        public void DefaultEcs_UpdateTPL() => _systemTPL.Update(1f / 60f);
    }
}
