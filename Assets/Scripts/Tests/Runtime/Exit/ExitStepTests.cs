using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class ExitStepTests
    {
        private static readonly ExitSettings Settings = new ExitSettings(0.08f, 10f, 0.5f);

        [Test]
        public void ExitStep_CompletesAndDisablesTheBlock()
        {
            var block = new Block(0, new Cell(1, 1), new[] { new Cell(0, 0), new Cell(0, 1) }, 0,
                Array.Empty<IModifier>());
            var path = new ExitPath(block, Direction.Up, Settings.ClipOffset);
            var view = new FakeExitView();
            var sfx = new FakeSfxPlayer();

            UniTask playing = new ExitStep(view, path, Settings, sfx).Play(CancellationToken.None);

            Assert.That(playing.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(view.Calls, Is.EqualTo(new[] { "clip", "move", "move", "burst 0", "move", "burst 1", "move", "hide" }));
            Assert.That(view.ClipPlane, Is.EqualTo(path.ClipPlane));
            Assert.That(sfx.Played, Is.EqualTo(new[] { SoundEffect.Crunch }));

            Assert.That(view.Targets[0], Is.EqualTo(path.Rest), "first it snaps onto its cell");
            Assert.That(view.Eases[0], Is.EqualTo(Ease.OutQuad));
            Assert.That(view.Targets[view.Targets.Count - 1], Is.EqualTo(path.Rest + path.Normal * path.TotalDistance),
                "it ends with its last row past the cut");
            Assert.That(view.Eases[1], Is.EqualTo(Ease.Linear), "the slide keeps one speed");
        }
    }
}
