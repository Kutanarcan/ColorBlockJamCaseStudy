using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class LevelDocumentTests
    {
        [Test]
        public void Save_IsBlocked_WhileARuleIsBroken()
        {
            LevelDocument document = TestSessions.Playable(new FakeReportingRule("broken")).Document;

            Result<string> saved = document.Save();

            Assert.That(saved.IsFailure, Is.True);
            Assert.That(document.Violations.Count, Is.EqualTo(1));
        }

        [Test]
        public void CheckRules_FailsWhileARuleIsBroken_WhateverTheKey()
        {
            LevelDocument document = TestSessions.New(new FakeReportingRule("broken")).Document;

            Assert.That(document.CheckRules().IsFailure, Is.True);
            Assert.That(TestSessions.New().Document.CheckRules().IsSuccess, Is.True);
        }

        [Test]
        public void Save_IsBlocked_ByAnInvalidKey()
        {
            LevelDocument document = TestSessions.Playable().Document;
            document.Key = "level 01/x";

            Assert.That(document.Save().IsFailure, Is.True);
        }

        [Test]
        public void SavedJson_OpensBack_IntoTheSameLevel()
        {
            LevelEditorSession session = TestSessions.Playable(new EdgeCellsRule(), new DoorWidthRule());
            session.Edits.AddModifier(session.Selected, "ice");
            string json = session.Document.Save().Value;

            LevelDocument reopened = TestSessions.New().Document;
            Result opened = reopened.Open("level-01", json);

            Assert.That(opened.IsSuccess, Is.True);
            Assert.That(reopened.Snapshot(), Is.EqualTo(json));
            Assert.That(reopened.IsDirty, Is.False);
        }

        [Test]
        public void Open_InvalidJson_Fails_AndKeepsTheCurrentLevel()
        {
            LevelDocument document = TestSessions.Playable().Document;
            LevelModel before = document.Model;

            Result opened = document.Open("broken", "{ not json");

            Assert.That(opened.IsFailure, Is.True);
            Assert.That(document.Model, Is.SameAs(before));
        }

        [Test]
        public void Restore_KeepsTheUnsavedState()
        {
            string snapshot = TestSessions.Playable().Document.Snapshot();

            LevelDocument restored = TestSessions.New().Document;
            restored.Restore("level-01", "", snapshot, wasDirty: true);

            Assert.That(restored.IsDirty, Is.True);
            Assert.That(restored.Key, Is.EqualTo("level-01"));
            Assert.That(restored.SavedKey, Is.Empty);
        }

        [Test]
        public void SavedKey_IsTheKeyTheFileWasOpenedOrSavedUnder()
        {
            LevelDocument document = TestSessions.Playable().Document;
            Assert.That(document.SavedKey, Is.Empty);

            document.MarkSaved();
            Assert.That(document.SavedKey, Is.EqualTo("level-01"));

            document.Open("level-02", document.Snapshot());
            Assert.That(document.SavedKey, Is.EqualTo("level-02"));
        }

        [Test]
        public void Opening_ClearsTheUndoHistory()
        {
            LevelDocument document = TestSessions.Playable().Document;
            Assert.That(document.CanUndo, Is.True);

            document.Open("level-01", document.Snapshot());

            Assert.That(document.CanUndo, Is.False);
        }

        [Test]
        public void LevelKey_AllowsOnlyFileSafeCharacters()
        {
            Assert.That(LevelKey.IsValid("level_01-b"), Is.True);
            Assert.That(LevelKey.IsValid(""), Is.False);
            Assert.That(LevelKey.IsValid("../level"), Is.False);
        }
    }
}
