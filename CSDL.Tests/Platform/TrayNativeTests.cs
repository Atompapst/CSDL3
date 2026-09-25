// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL;
using CSDL.Video;
using CSDL3.Tests.TestSupport;
using Assert = Xunit.Assert;

namespace CSDL3.Tests.Platform {
    [Collection(SdlCollection.Name)]
    public sealed class TrayNativeTests {
        // The bundled SDL3.dll (3.4.16) returns NULL from SDL_CreateTray without ever setting an
        // SDL error when both the icon and the tooltip are NULL - so every test that needs a
        // working tray passes a tooltip and asserts IsValid right away, rather than discovering the
        // failure several calls later as a confusing ObjectDisposedException.
        private const string Tooltip = "CSDL3 Tray Test";

        private static Tray CreateValidTray() {
            Tray tray = new Tray(default, Tooltip);
            Assert.True(tray.IsValid, CSDL.Error.GetError());
            return tray;
        }

        [Fact]
        public void Constructor_WithoutIcon_CreatesValidTray() {
            using Tray tray = new Tray(default, Tooltip);

            Assert.True(tray.IsValid, CSDL.Error.GetError());
            Assert.NotEqual(nint.Zero, tray.NativePointer);
        }

        [Fact]
        public void Constructor_WithIcon_CreatesValidTray_AndDoesNotConsumeTheIcon() {
            using Surface icon = new Surface(2, 2, PixelFormat.RGBA8888);
            using Tray tray = new Tray(icon, Tooltip);

            Assert.True(tray.IsValid, CSDL.Error.GetError());
            Assert.True(icon.IsValid, "the icon surface must still be usable after tray creation - SDL only reads it during the call.");
        }

        [Fact]
        public void SetIcon_WithDefaultSurface_RemovesIconWithoutThrowing() {
            using Surface icon = new Surface(2, 2, PixelFormat.RGBA8888);
            using Tray tray = new Tray(icon, Tooltip);

            // Regression: SetIcon(default) used to dereference the null `Surface` before checking it.
            Exception? ex = Record.Exception(() => tray.SetIcon(default));

            Assert.Null(ex);
        }

        [Fact]
        public void Dispose_InvalidatesHandle_AndIsIdempotent() {
            Tray tray = CreateValidTray();

            tray.Dispose();
            Assert.False(tray.IsValid);
            Assert.Equal(nint.Zero, tray.NativePointer);

            Exception? ex = Record.Exception(() => tray.Dispose());
            Assert.Null(ex);
        }

        [Fact]
        public void Dispose_ThenCreateMenu_ThrowsObjectDisposedException() {
            Tray tray = CreateValidTray();

            tray.Dispose();

            Assert.Throws<ObjectDisposedException>(() => tray.CreateMenu());
        }

        [Fact]
        public void CreateMenu_MenuProperty_AndGetOrCreateMenu_AllResolveToTheSameNativeMenu() {
            using Tray tray = CreateValidTray();

            TrayMenu created = tray.CreateMenu();
            Assert.True(created.IsValid);

            TrayMenu? fetched = tray.Menu;
            Assert.NotNull(fetched);
            Assert.Equal(created.NativePointer, fetched!.NativePointer);

            TrayMenu getOrCreate = tray.GetOrCreateMenu();
            Assert.Equal(created.NativePointer, getOrCreate.NativePointer);
        }

        [Fact]
        public void Menu_ParentTray_ResolvesToTheSameHandleAsItsOwningTray() {
            using Tray tray = CreateValidTray();
            TrayMenu menu = tray.CreateMenu();

            Tray parent = menu.ParentTray;

            Assert.True(parent.IsValid);
            Assert.Equal(tray.NativePointer, parent.NativePointer);
            Assert.Equal(tray, parent);
        }

        [Fact]
        public void Menu_Add_CreatesEntryOwnedByTray_ThatGoesInvalidOnceTheTrayIsDisposed() {
            Tray tray = CreateValidTray();
            TrayMenu menu = tray.CreateMenu();

            TrayEntry entry = menu.Add("Quit");
            Assert.True(entry.IsValid);
            Assert.Equal("Quit", entry.Label);

            entry.Enabled = false;
            Assert.False(entry.Enabled);

            tray.Dispose();

            Assert.False(entry.IsValid);

            // SDL already tore the entry down together with the tray; disposing the wrapper must be a no-op.
            Exception? ex = Record.Exception(() => entry.Dispose());
            Assert.Null(ex);
        }

        [Fact]
        public void Menu_Entries_ReflectsInsertedEntriesInOrder() {
            using Tray tray = CreateValidTray();
            TrayMenu menu = tray.CreateMenu();

            menu.Add("First");
            menu.AddSeparator();
            menu.AddCheckbox("Second", @checked: true);

            TrayEntry[] entries = menu.Entries;

            Assert.Equal(3, entries.Length);
            Assert.Equal("First", entries[0].Label);
            Assert.True(string.IsNullOrEmpty(entries[1].Label), "a separator's label should read back as null or empty.");
            Assert.Equal("Second", entries[2].Label);
            Assert.True(entries[2].Checked);
        }

        [Fact]
        public void AddSubmenu_CreatesSubmenuWhoseParentEntryMatchesAndHasNoParentTray() {
            using Tray tray = CreateValidTray();
            TrayMenu menu = tray.CreateMenu();

            TrayMenu submenu = menu.AddSubmenu("More");

            Assert.True(submenu.IsValid);

            TrayEntry parentEntry = submenu.ParentEntry;
            Assert.True(parentEntry.IsValid);
            Assert.Equal("More", parentEntry.Label);

            Assert.False(submenu.ParentTray.IsValid);
        }

        [Fact]
        public void TrayEntry_Click_InvokesRegisteredCallback_AndRemoveCallbackStopsIt() {
            using Tray tray = CreateValidTray();
            TrayMenu menu = tray.CreateMenu();
            TrayEntry entry = menu.Add("Ping");

            int callCount = 0;
            object userdata = new object();
            object? capturedUserdata = null;
            nint capturedEntryPointer = 0;

            entry.SetCallback((data, clickedEntry) => {
                callCount++;
                capturedUserdata = data;
                capturedEntryPointer = clickedEntry.NativePointer;
            }, userdata);

            entry.Click();

            Assert.Equal(1, callCount);
            Assert.Same(userdata, capturedUserdata);
            Assert.Equal(entry.NativePointer, capturedEntryPointer);

            entry.RemoveCallback();
            entry.Click();

            Assert.Equal(1, callCount);
        }

        [Fact]
        public void TrayEntry_RemoveCallback_WithoutPriorCallback_IsNoOp() {
            using Tray tray = CreateValidTray();
            TrayMenu menu = tray.CreateMenu();
            TrayEntry entry = menu.Add("X");

            Exception? ex = Record.Exception(() => entry.RemoveCallback());
            Assert.Null(ex);
        }

        [Fact]
        public void Constructor_WithNullProperties_ThrowsArgumentNullException() {
            Assert.Throws<ArgumentNullException>(() => new Tray((TrayCreateProperties)null!));
        }

        // TODO: UNCOMMENT WHEN SDL 3.6.0 is stable
        // These two require SDL_CreateTrayWithProperties, added in SDL 3.6.0. The SDL3.dll currently
        // bundled under CSDL.Tests reports as 3.4.16 and is missing the export, so both fail with an
        // EntryPointNotFoundException until that native library is updated - see the summary.
        // [Fact]
        // public void Constructor_WithProperties_CreatesValidTray() {
        //     using TrayCreateProperties properties = new TrayCreateProperties();
        //     properties.Tooltip.Set(Tooltip);
        //
        //     using Tray tray = new Tray(properties);
        //
        //     Assert.True(tray.IsValid, CSDL.Error.GetError());
        // }
        //
        // [Fact]
        // public void Constructor_WithProperties_ClickCallback_CreatesValidTray_AndDisposesWithoutThrowing() {
        //     using TrayCreateProperties properties = new TrayCreateProperties();
        //     properties.SetLeftClickCallback((_, _) => true);
        //
        //     using Tray tray = new Tray(properties);
        //
        //     Assert.True(tray.IsValid, CSDL.Error.GetError());
        // }
    }
}
