using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class ModifierFieldsTests
    {
        [Test]
        public void Of_ListsTheFieldsTheModifierWrites()
        {
            ModifierFields fields = ModifierFields.Of(new IceData { Count = 3 });

            Assert.That(fields.Count, Is.EqualTo(1));
            Assert.That(fields.KeyAt(0), Is.EqualTo("count"));
            Assert.That(fields.KindAt(0), Is.EqualTo(FieldKind.Int));
            Assert.That(fields.IntAt(0), Is.EqualTo(3));
        }

        [Test]
        public void EditedValues_AreAppliedBack_ThroughRead()
        {
            var ice = new IceData { Count = 3 };
            var arrow = new ArrowData { Axis = Axis.Vertical };
            ModifierFields iceFields = ModifierFields.Of(ice);
            ModifierFields arrowFields = ModifierFields.Of(arrow);

            iceFields.SetInt(0, 7);
            arrowFields.SetAxis(0, Axis.Horizontal);
            iceFields.ApplyTo(ice);
            arrowFields.ApplyTo(arrow);

            Assert.That(ice.Count, Is.EqualTo(7));
            Assert.That(arrow.Axis, Is.EqualTo(Axis.Horizontal));
        }
    }
}
