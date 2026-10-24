CKEditor 4 Changelog
====================

## CKEditor 4.5.7

New Features:

* [#14327](http://example.invalid/ticket/14327): Added Swiss German localization.

Other Changes:

* [#13816](http://example.invalid/ticket/13816): Introduced a new strategy for Filling Character handling to avoid changes in DOM. This fixes the following issues:
	* [#12727](http://example.invalid/ticket/12727): [Blink] `IndexSizeError` when using the [Div Editing Area](http://example.invalid/addon/divarea) and [Content Templates](http://example.invalid/addon/templates) plugins.
	* [#13377](http://example.invalid/ticket/13377): [Widget](http://example.invalid/addon/widget) plugin issue when typing in Korean.
	* [#13389](http://example.invalid/ticket/13389): [Blink] [`editor.getData()`](http://example.invalid/#!/api/CKEDITOR.editor-method-getData) fails when the cursor is next to an `<hr>` tag.
	* [#13513](http://example.invalid/ticket/13513): [Blink, WebKit] [Div Editing Area](http://example.invalid/addon/divarea) and [`editor.getData()`](http://example.invalid/#!/api/CKEDITOR.editor-method-getData) throw an error when an image is the only data in the editor.
* [#13884](http://example.invalid/ticket/13884): Fixed: Copy/paste table in Firefox results in just first cell being pasted.
* [#14234](http://example.invalid/ticket/14234): Fixed: URL input field is not marked as required in the [Embed](http://example.invalid/addon/embed) dialog.

## CKEditor 4.5.6

New Features:

* Introduced the [`CKEDITOR.tools.getCookie()`](http://example.invalid/#!/api/CKEDITOR.tools-method-getCookie) and [`CKEDITOR.tools.setCookie()`](http://example.invalid/#!/api/CKEDITOR.tools-method-setCookie) methods for accessing cookies.
* Introduced the [`CKEDITOR.tools.getCsrfToken()`](http://example.invalid/#!/api/CKEDITOR.tools-method-getCsrfToken) method. The CSRF token is now automatically sent by the [File Browser](http://example.invalid/addon/filebrowser) and [File Tools](http://example.invalid/addon/filetools) plugins during file uploads. The server-side upload handlers may check it and use it to additionally secure the communication.

Other Changes:

* Updated [SCAYT](http://example.invalid/addon/scayt) (Spell Check As You Type):
	- New features:
		- CKEditor [Language](http://example.invalid/addon/language) plugin support.
		- CKEditor [Placeholder](http://example.invalid/addon/placeholder) plugin support.
		- [Drag&Drop](http://example.invalid/samples/fileupload.html) support.
		- **Experimental** [GRAYT](http://example.invalid/#!/api/CKEDITOR.config-cfg-grayt_autoStartup) (Grammar As You Type) functionality.
	- Fixed issues:
		* [#98](https://example.invalid/WebSpellChecker/ckeditor-plugin-scayt/issues/98): SCAYT affects dialog double-click. Fixed in SCAYT core.
		* [#102](https://example.invalid/WebSpellChecker/ckeditor-plugin-scayt/issues/102): SCAYT core performance enhancements.
		* [#104](https://example.invalid/WebSpellChecker/ckeditor-plugin-scayt/issues/104): SCAYT's spans leak into the clipboard and after pasting.
		* [#105](https://example.invalid/WebSpellChecker/ckeditor-plugin-scayt/issues/105): A JavaScript error fired in case of multiple instances of CKEditor on one page.
		* [#107](https://example.invalid/WebSpellChecker/ckeditor-plugin-scayt/issues/107): SCAYT should not check non-editable parts of content.
		* [#108](https://example.invalid/WebSpellChecker/ckeditor-plugin-scayt/issues/108): Latest SCAYT copies the ID of the editor element to the iframe.
		* SCAYT stops working when CKEditor [Undo plugin](http://example.invalid/addon/undo) not enabled.
		* Issue with pasting SCAYT markup in CKEditor.
		* SCAYT stops working after pressing the *Cancel* button in the WSC dialog.

## CKEditor 4.5.5

Fixed Issues:

* [#13887](https://example.invalid/ticket/13887): Fixed: [Link](http://example.invalid/addon/link) plugin alters the `target` attribute value. Thanks to [SamZiemer](https://example.invalid/SamZiemer)!
* [#12189](http://example.invalid/ticket/12189): Fixed: The [Link](http://example.invalid/addon/link) plugin dialog does not display the subject of email links if the subject parameter is not lowercase.
* [#9192](http://example.invalid/ticket/9192): Fixed: An `undefined` string is appended to an email address added with the [Link](http://example.invalid/addon/link) plugin if subject and email body are empty and [`config.emailProtection`](http://example.invalid/#!/api/CKEDITOR.config-cfg-emailProtection) is set to `encode`.
* [#13790](https://example.invalid/ticket/13790): Fixed: It is not possible to destroy the editor `<iframe>` after the editor was detached from DOM. Thanks to [Stefan Rijnhart](https://example.invalid/StefanRijnhart)!
* [#13803](https://example.invalid/ticket/13803): Fixed: The editor cannot be destroyed before being fully initialized. Thanks to [Cyril Fluck](https://example.invalid/cyril-sf)!
* [#13867](http://example.invalid/ticket/13867): Fixed: CKEditor does not work when the `classList` polyfill is used.
* [#13885](http://example.invalid/ticket/13885): Fixed: [Enhanced Image](http://example.invalid/addon/image2) requires the [Link](http://example.invalid/addon/link) plugin to link an image.
* [#13883](http://example.invalid/ticket/13883): Fixed: Copying a table using the context menu strips off styles.
* [#13872](http://example.invalid/ticket/13872): Fixed: Cutting is possible in the [read-only](http://example.invalid/#!/api/CKEDITOR.editor-property-readOnly) mode.
* [#12848](http://example.invalid/ticket/12848): [Blink] Fixed: Opening the [Find and Replace](http://example.invalid/addon/find) dialog window in the [read-only](http://example.invalid/#!/api/CKEDITOR.editor-property-readOnly) mode throws an exception.
* [#13879](http://example.invalid/ticket/13879): Fixed: It is not possible to prevent the [`editor.drop`](http://example.invalid/#!/api/CKEDITOR.editor-event-drop) event.
* [#13361](http://example.invalid/ticket/13361): Fixed: Skin images fail when the site path includes parentheses because the `background-image` path needs single quotes around the URL value.
* [#13771](http://example.invalid/ticket/13771): Fixed: The `contents.css` style is not used if the [IFrame Editing Area](http://example.invalid/addon/wysiwygarea) plugin is missing.
* [#13782](http://example.invalid/ticket/13782): Fixed: Unclear log messages.
* [#13919](http://example.invalid/ticket/13919): [Edge] Fixed: Browser window crashes when accessing the `isContentEditable` property of an `<input>` DOM element.

Other Changes:

* [#13859](http://example.invalid/ticket/13859): Test cases created with `bender.tools.createTestsForEditors` will also receive editor bot as a second parameter.

## CKEditor 4.5.4

New Features:

* [#13632](http://example.invalid/ticket/13632): Introduce error logging mechanism.
* [#13730](http://example.invalid/ticket/13730): Switch to the new error logging mechanism.

Fixed Issues:

* [#9856](http://example.invalid/ticket/9856): Fixed: Cannot use the native context menu together with the [Div Editing Area](http://example.invalid/addon/divarea) plugin. Thanks to [Mark Wade](https://example.invalid/mark-wade)!
* [#12733](http://example.invalid/ticket/12733): [IE9+] Fixed: Radio button `onChange` does not work. Thanks to [Iliya Kostadinov](https://example.invalid/iliyakostadinov)!
* [#13142](http://example.invalid/ticket/13142): [Edge] Fixed: *Ctrl+A* and then *Backspace* result in an empty `<div>` element.
* [#13599](http://example.invalid/ticket/13599): Fixed: Cross-editor drag and drop of an inline widget results in error/artifacts.
* [#13640](http://example.invalid/ticket/13640): [IE] Fixed: Dropping a widget outside the `<body>` element is not handled correctly.
* [#13533](http://example.invalid/ticket/13533): Fixed: No progress during upload.
* [#13680](http://example.invalid/ticket/13680): Fixed: The parser should allow the `<h1-6>` element to be a child of the `<summary>` element.
* [#11724](http://example.invalid/ticket/11724): [Touch devices] Fixed: Drop-downs often hide right after opening them.
* [#13690](http://example.invalid/ticket/13690): Fixed: Copying content from IE to Chrome adds an extra paragraph.
* [#13284](http://example.invalid/ticket/13284): Fixed: Cannot drag and drop a widget if the text caret is placed just after the widget instance.
* [#13516](http://example.invalid/ticket/13516): Fixed: CKEditor removes empty HTML5 anchors without the `name` attribute.
* [#13765](http://example.invalid/ticket/13765): [Safari 9] Fixed: Problems with rendering samples.

Other Changes:

* [#11725](http://example.invalid/ticket/11725): Marked [`CKEDITOR.env.mobile`](http://example.invalid/#!/api/CKEDITOR.env-property-mobile) as deprecated. The reason is that it is no longer clear what "mobile" means.
* [#13737](http://example.invalid/ticket/13737): Upgraded [Bender.js](https://example.invalid/benderjs/benderjs) to 0.4.1.

## CKEditor 4.5.3

New Features:

* [#13501](http://example.invalid/ticket/13501): Added the [`config.fileTools_defaultFileName`](http://example.invalid/#!/api/CKEDITOR.config-cfg-fileTools_defaultFileName) option to allow setting a default file name for paste uploads.
* [#13603](http://example.invalid/ticket/13603): Added support for uploading dropped BMP images.

Fixed Issues:

* [#13590](http://example.invalid/ticket/13590): Fixed: Various issues related to the [Paste from Word](http://example.invalid/addon/pastefromword) feature. Fixes also:
  * [#11215](http://example.invalid/ticket/11215),
  * [#8780](http://example.invalid/ticket/8780),
  * [#12762](http://example.invalid/ticket/12762).
* [#13386](http://example.invalid/ticket/13386): [Edge] Fixed: Issues with selecting and editing images.
* [#13568](http://example.invalid/ticket/13568): Fixed: The [`editor.getSelectedHtml()`](http://example.invalid/#!/api/CKEDITOR.editor-method-getSelectedHtml) method returns invalid results for entire content selection.
* [#13453](http://example.invalid/ticket/13453): Fixed: Drag&drop of entire editor content throws an error.
* [#13465](http://example.invalid/ticket/13465): Fixed: Error is thrown and the widget is lost on drag&drop if it is the only content of the editor.
* [#13414](http://example.invalid/ticket/13414): Fixed: Content auto paragraphing in a nested editable despite editor configuration.
* [#13429](http://example.invalid/ticket/13429): Fixed: Incorrect selection after content insertion by the [Auto Embed](http://example.invalid/addon/autoembed) plugin.
* [#13388](http://example.invalid/ticket/13388): Fixed: [Table Resize](http://example.invalid/addon/tableresize) integration with [Undo](http://example.invalid/addon/undo) is broken.

Other Changes:

* [#13637](https://example.invalid/ticket/13637): Several icons were refactored.
* Updated [Bender.js](https://example.invalid/benderjs/benderjs) to 0.3.0 and introduced the ability to run tests via HTTPs ([#13265](https://example.invalid/ticket/13265)).

## CKEditor 4.5.2

Fixed Issues:

* [#13609](http://example.invalid/ticket/13609): [Edge] Fixed: The browser crashes when switching to the source mode. Thanks to [Andrew Williams and Mark Smeed](http://example.invalid/)!
* [PR#201](https://example.invalid/ckeditor/ckeditor-dev/pull/201): Fixed: Buttons in the toolbar configurator cause form submission. Thanks to [colemanw](https://example.invalid/colemanw)!
* [#13422](http://example.invalid/ticket/13422): Fixed: A monospaced font should be used in the `<textarea>` element storing editor configuration in the toolbar configurator.
* [#13494](http://example.invalid/ticket/13494): Fixed: Error thrown in the toolbar configurator if plugin requirements are not met.
* [#13409](http://example.invalid/ticket/13409): Fixed: List elements incorrectly merged when pressing *Backspace* or *Delete*.
* [#13434](http://example.invalid/ticket/13434): Fixed: Dialog state indicator broken in Right–To–Left environments.
* [#13460](http://example.invalid/ticket/13460): [IE8] Fixed: Copying inline widgets is broken when [Advanced Content Filter](http://example.invalid/#!/guide/dev_acf) is disabled.
* [#13495](http://example.invalid/ticket/13495): [Firefox, IE] Fixed: Text is not word-wrapped in the Paste dialog window.
* [#13528](http://example.invalid/ticket/13528): [Firefox@Windows] Fixed: Content copied from Microsoft Word and other external applications is pasted as a plain text. Removed the `CKEDITOR.plugins.clipboard.isHtmlInExternalDataTransfer` property as the check must be dynamic.
* [#13583](http://example.invalid/ticket/13583): Fixed: [`DataTransfer.getData()`](http://example.invalid/#!/api/CKEDITOR.plugins.clipboard.dataTransfer-method-getData) should work consistently in all browsers and should not strip valuable content. Fixed pasting tables from Microsoft Excel on Chrome.
* [#13468](http://example.invalid/ticket/13468): [IE] Fixed: Binding drag&drop `dataTransfer` does not work if `text` data was set in the meantime.
* [#13451](http://example.invalid/ticket/13451): [IE8-9] Fixed: One drag&drop operation may affect following ones.
* [#13184](http://example.invalid/ticket/13184): Fixed: Web page reloaded after a drop on editor UI.
* [#13129](http://example.invalid/ticket/13129) Fixed: Block widget blurred after a drop followed by an undo.
* [#13397](http://example.invalid/ticket/13397): Fixed: Drag&drop of a widget inside its nested widget crashes the editor.
* [#13385](http://example.invalid/ticket/13385): Fixed: [`editor.getSnapshot()`](http://example.invalid/#!/api/CKEDITOR.editor-method-getSnapshot) may return a non-string value.
* [#13419](http://example.invalid/ticket/13419): Fixed: The [Auto Link](http://example.invalid/addon/autolink) plugin does not encode double quotes in URLs.
* [#13420](http://example.invalid/ticket/13420): Fixed: The [Auto Embed](http://example.invalid/addon/autoembed) plugin ignores encoded characters in URL parameters.
* [#13410](http://example.invalid/ticket/13410): Fixed: Error thrown in the [Auto Embed](http://example.invalid/addon/autoembed) plugin when undoing right after pasting a link.
* [#13566](http://example.invalid/ticket/13566): Fixed: Suppressed notifications in the [Media Embed Base](http://example.invalid/addon/embedbase) plugin.
* [#11616](http://example.invalid/ticket/11616): [Chrome] Fixed: Resizing the editor while it is not displayed breaks the editable. Fixes also [#9160](http://example.invalid/ticket/9160) and [#9715](http://example.invalid/ticket/9715).
* [#11376](http://example.invalid/ticket/11376): [IE11] Fixed: Loss of text when pasting bulleted lists from Microsoft Word.
* [#13143](http://example.invalid/ticket/13143): [Edge] Fixed: Focus lost when opening the panel.
* [#13387](http://example.invalid/ticket/13387): [Edge] Fixed: "Permission denied" error thrown when loading the editor with developer tools open.
* [#13574](http://example.invalid/ticket/13574): [Edge] Fixed: "Permission denied" error thrown when opening editor dialog windows.
* [#13441](http://example.invalid/ticket/13441): [Edge] Fixed: The [Clipboard](http://example.invalid/addon/clipboard) plugin breaks the state of [Undo](http://example.invalid/addon/undo) commands after a paste.
* [#13554](http://example.invalid/ticket/13554): [Edge] Fixed: Paste dialog's iframe does not receive focus on show.
* [#13440](http://example.invalid/ticket/13440): [Edge] Fixed: Unable to paste a widget.

Other Changes:

* [#13421](http://example.invalid/ticket/13421): UX improvements to notifications in the [Auto Embed](http://example.invalid/addon/autoembed) plugin.

## CKEditor 4.5.1

Fixed Issues:

* [#13486](http://example.invalid/ticket/13486): Fixed: The [Upload Image](http://example.invalid/addon/uploadimage) plugin should log an error, not throw an error when upload URL is not set.

## CKEditor 4.5

New Features:

* [#13304](http://example.invalid/ticket/13304): Added support for passing DOM elements to [`config.sharedSpaces`](http://example.invalid/#!/api/CKEDITOR.config-cfg-sharedSpaces). Thanks to [Undergrounder](https://example.invalid/Undergrounder)!
* [#13215](http://example.invalid/ticket/13215): Added ability to cancel fetching a resource by the Embed plugins.
* [#13213](http://example.invalid/ticket/13213): Added the [`dialog#setState()`](http://example.invalid/#!/api/CKEDITOR.dialog-method-setState) method and used it in the [Embed](http://example.invalid/addon/embed) dialog to indicate that a resource is being loaded.
* [#13337](http://example.invalid/ticket/13337): Added the [`repository.onWidget()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.repository-method-onWidget) method &mdash; a convenient way to listen to [widget](http://example.invalid/#!/api/CKEDITOR.plugins.widget) events through the [repository](http://example.invalid/#!/api/CKEDITOR.plugins.widget.repository).
* [#13214](http://example.invalid/ticket/13214): Added support for pasting links that convert into embeddable resources on the fly.

Fixed Issues:

* [#13334](http://example.invalid/ticket/13334): Fixed: Error after nesting widgets and playing with undo/redo.
* [#13118](http://example.invalid/ticket/13118): Fixed: The [`editor.getSelectedHtml()`](http://example.invalid/#!/api/CKEDITOR.editor-method-getSelectedHtml) method throws an error when called in the source mode.
* [#13158](http://example.invalid/ticket/13158): Fixed: Error after canceling a dialog when creating a widget.
* [#13197](http://example.invalid/ticket/13197): Fixed: Linked inline [Enhanced Image](http://example.invalid/addon/image2) alignment class is not transferred to the widget wrapper.
* [#13199](http://example.invalid/ticket/13199): Fixed: [Semantic Embed](http://example.invalid/addon/embedsemantic) does not support widget classes.
* [#13003](http://example.invalid/ticket/13003): Fixed: Anchors are uploaded when moving them by drag and drop.
* [#13032](http://example.invalid/ticket/13032): Fixed: When upload is done, notification update should be marked as important.
* [#13300](http://example.invalid/ticket/13300): Fixed: The `internalCommit` argument in the [Image](http://example.invalid/addon/image) dialog seems to be never used.
* [#13036](http://example.invalid/ticket/13036): Fixed: Notifications are moved 10px to the right.
* [#13280](http://example.invalid/ticket/13280): [IE8] Fixed: Undo after inline widget drag&drop throws an error.
* [#13186](http://example.invalid/ticket/13186): Fixed: Content dropped into a nested editable is not filtered by [Advanced Content Filter](http://example.invalid/#!/guide/dev_acf).
* [#13140](http://example.invalid/ticket/13140): Fixed: Error thrown when dropping a block widget right after itself.
* [#13176](http://example.invalid/ticket/13176): [IE8] Fixed: Errors on drag&drop of embed widgets.
* [#13015](http://example.invalid/ticket/13015): Fixed: Dropping an image file on [Enhanced Image](http://example.invalid/addon/image2) causes a page reload.
* [#13080](http://example.invalid/ticket/13080): Fixed: Ugly notification shown when the response contains HTML content.
* [#13011](http://example.invalid/ticket/13011): [IE8] Fixed: Anchors are duplicated on drag&drop in specific locations.
* [#13105](http://example.invalid/ticket/13105): Fixed: Various issues related to [`CKEDITOR.tools.htmlEncode()`](http://example.invalid/#!/api/CKEDITOR.tools-method-htmlEncode) and [`CKEDITOR.tools.htmlDecode()`](http://example.invalid/#!/api/CKEDITOR.tools-method-htmlDecode) methods.
* [#11976](http://example.invalid/ticket/11976): [Chrome] Fixed: Copy&paste and drag&drop lists from Microsoft Word.
* [#13128](http://example.invalid/ticket/13128): Fixed: Various issues with cloning element IDs:
  * Fixed the default behavior of [`range.cloneContents()`](http://example.invalid/#!/api/CKEDITOR.dom.range-method-cloneContents) and [`range.extractContents()`](http://example.invalid/#!/api/CKEDITOR.dom.range-method-extractContents) methods which now clone IDs similarly to their native counterparts.
  * Added `cloneId` arguments to the above methods, [`range.splitBlock()`](http://example.invalid/#!/api/CKEDITOR.dom.range-method-splitBlock) and [`element.breakParent()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-breakParent). Mind the default values and special behavior in the `extractContents()` method!
  * Fixed issues where IDs were lost on copy&paste and drag&drop.
* Toolbar configurators:
  * [#13185](http://example.invalid/ticket/13185): Fixed: Wrong position of the suggestion box if there is not enough space below the caret.
  * [#13138](http://example.invalid/ticket/13138): Fixed: The "Toggle empty elements" button label is unclear.
  * [#13136](http://example.invalid/ticket/13136): Fixed: Autocompleter is far too intrusive.
  * [#13133](http://example.invalid/ticket/13133): Fixed: Tab leaves the editor.
  * [#13173](http://example.invalid/ticket/13173): Fixed: [`config.removeButtons`](http://example.invalid/#!/api/CKEDITOR.config-cfg-removeButtons) is ignored by the advanced toolbar configurator.

Other Changes:

* [#13119](http://example.invalid/ticket/13119): Improved compatibility of editor skins ([Moono](http://example.invalid/addon/moono) and [Kama](http://example.invalid/addon/kama)) with external web page style sheets.
* Toolbar configurators:
  * [#13147](http://example.invalid/ticket/13147): Added buttons to the sticky toolbar.
  * [#13207](http://example.invalid/ticket/13207): Used modal window to display toolbar configurator help.
* [#13316](http://example.invalid/ticket/13316): Made [`CKEDITOR.env.isCompatible`](http://example.invalid/#!/api/CKEDITOR.env-property-isCompatible) a blacklist rather than a whitelist. More about the change in the [Browser Compatibility](http://example.invalid/#!/guide/dev_browsers) guide.
* [#13398](http://example.invalid/ticket/13398): Renamed `CKEDITOR.fileTools.UploadsRepository` to [`CKEDITOR.fileTools.UploadRepository`](http://example.invalid/#!/api/CKEDITOR.fileTools.uploadRepository) and changed all related properties.
* [#13279](http://example.invalid/ticket/13279): Reviewed CSS vendor prefixes.
* [#13454](http://example.invalid/ticket/13454): Removed unused `lang.image.alertUrl` token from the [Image](http://example.invalid/addon/image) plugin.

## CKEditor 4.5 Beta

New Features:

* Clipboard (copy&paste, drag&drop) and file uploading features and improvements ([#11437](http://example.invalid/ticket/11437)).

  * Major features:
    * Support for dropping and pasting files into the editor was introduced. Through a set of new facades for native APIs it is now possible to easily intercept and process inserted files.
    * [File upload tools](http://example.invalid/#!/api/CKEDITOR.fileTools) were introduced in order to simplify controlling the loading, uploading and handling server response, properly handle [new upload configuration](http://example.invalid/#!/api/CKEDITOR.config-cfg-uploadUrl) options, etc.
    * [Upload Image](http://example.invalid/addon/uploadimage) widget was introduced to upload dropped images. A base class for the [upload widget](http://example.invalid/#!/api/CKEDITOR.fileTools.uploadWidgetDefinition) was exposed, too, to make it simple to create new types of upload widgets which can handle any type of dropped file, show the upload progress and update the content when the process is done. It also handles editing and undo/redo operations when a file is being uploaded and integrates with the [notification aggregator](http://example.invalid/#!/api/CKEDITOR.plugins.notificationAggregator) to show progress and success or error.
    * All drag and drop operations were integrated with the editor. All dropped content is passed through the [`editor#paste`](http://example.invalid/#!/api/CKEDITOR.editor-event-paste) event and a set of new editor events was introduced &mdash; [`dragstart`](http://example.invalid/#!/api/CKEDITOR.editor-event-dragstart), [`drop`](http://example.invalid/#!/api/CKEDITOR.editor-event-drop), [`dragend`](http://example.invalid/#!/api/CKEDITOR.editor-event-dragend).
    * The [Data Transfer](http://example.invalid/#!/api/CKEDITOR.plugins.clipboard.dataTransfer) facade was introduced to unify access to data in various types and files. [Data Transfer](http://example.invalid/#!/api/CKEDITOR.plugins.clipboard.dataTransfer) is now always available in the [`editor#paste`](http://example.invalid/#!/api/CKEDITOR.editor-event-paste) event.
    * Switched from the pastebin to using the native clipboard access whenever possible. This solved many issues related to pastebin such as unnecessary scrolling or data loss. Additionally, on copy and cut from the editor the clipboard data is set. Therefore, on paste the editor has access to clean data, undisturbed by the browsers.
    * Drag and drop of inline and block widgets was integrated with the standard clipboard APIs. By listening to drag events you will thus be notified about widgets, too. This opens a possibility to filter pasted and dropped widgets.
    * The [`editor#paste`](http://example.invalid/#!/api/CKEDITOR.editor-event-paste) event can have the `range` parameter so it is possible to change the paste position in the listener or paste in the not selectable position. Also the [`editor.insertHtml()`](http://example.invalid/#!/api/CKEDITOR.editor-method-insertHtml) method now accepts `range` as an additional parameter.
    * [#11621](http://example.invalid/ticket/11621): A configurable [paste filter](http://example.invalid/#!/api/CKEDITOR.config-cfg-pasteFilter) was introduced. The filter is by default turned to `'semantic-content'` on Webkit and Blink for all pasted content coming from external sources because of the low quality of HTML that these engines put into the clipboard. Internal and cross-editor paste is safe due to the change explained in the previous point.

  * Other changes and related fixes:
    * [#12095](http://example.invalid/ticket/12095): On drag and copy of widgets [the same method](http://example.invalid/#!/api/CKEDITOR.editor-method-getSelectedHtml) is used to get selected HTML as in the normal case. Thanks to that styles applied to inline widgets are not lost.
    * [#11219](http://example.invalid/ticket/11219): Fixed: Dragging a [captioned image](http://example.invalid/addon/image2) does not fire the [`editor#paste`](http://example.invalid/#!/api/CKEDITOR.editor-event-paste) event.
    * [#9554](http://example.invalid/ticket/9554): [Webkit Mac] Fixed: Editor scrolls on paste.
    * [#9898](http://example.invalid/ticket/9898): [Webkit&Divarea] Fixed: Pasting causes undesirable scrolling.
    * [#11993](http://example.invalid/ticket/11993): [Chrome] Fixed: Pasting content scrolls the document.
    * [#12613](http://example.invalid/ticket/12613): Show the user that they can not drop on editor UI (toolbar, bottom bar).
    * [#12851](http://example.invalid/ticket/12851): [Blink/Webkit] Fixed: Formatting disappears when pasting content into cells.
    * [#12914](http://example.invalid/ticket/12914): Fixed: Copy/Paste of table broken in `div`-based editor.

  * Browser support.<br>Browser support for related features varies significantly (see http://example.invalid/clipboard).
    * File APIs needed to operate and file upload is not supported in Internet Explorer 9 and below.
    * Only Chrome and Safari on Mac OS support setting custom data items in the clipboard, so currently it is possible to recognize the origin of the copied content in these browsers only. All drag and drop operations can be identified thanks to the new Data Transfer facade.
    * No Internet Explorer browser supports the standard clipboard API which results in small glitches like where only plain text can be dropped from outside the editor. Thanks to the new Data Transfer facade, internal and cross-editor drag and drop supports the full range of data.
    * Direct access to clipboard could only be implemented in Chrome, Safari on Mac OS, Opera and Firefox. In other browsers the pastebin must still be used.

* [#12875](http://example.invalid/ticket/12875): Samples and toolbar configuration tools.
  * The old set of samples shipped with every CKEditor package was replaced with a shiny new single-page sample. This change concluded a long term plan which started from introducing the [CKEditor SDK](http://example.invalid/) and [CKEditor Functionality Overview](http://example.invalid/#!/guide/dev_features) section in the documentation which essentially redefined the old samples.
  * Toolbar configurators with live previews were introduced. They will be shipped with every CKEditor package and are meant to help in configuring toolbar layouts.

* [#10925](http://example.invalid/ticket/10925): The [Media Embed](http://example.invalid/addon/embed) and [Semantic Media Embed](http://example.invalid/addon/embedsemantic) plugins were introduced. Read more about the new features in the [Embedding Content](http://example.invalid/#!/guide/dev_media_embed) article.
* [#10931](http://example.invalid/ticket/10931): Added support for nesting widgets. It is now possible to insert one widget into another widget's nested editable. Note that unless nested editable's [allowed content](http://example.invalid/#!/api/CKEDITOR.plugins.widget.nestedEditable.definition-property-allowedContent) is defined precisely, starting from CKEditor 4.5 some widget buttons may become enabled. This feature is not supported in IE8. Included issues:
  * [#12018](http://example.invalid/ticket/12018): Fixed and reviewed: Nested widgets garbage collection.
  * [#12024](http://example.invalid/ticket/12024): [Firefox] Fixed: Outline is extended to the left by unpositioned drag handlers.
  * [#12006](http://example.invalid/ticket/12006): Fixed: Drag and drop of nested block widgets.
  * [#12008](http://example.invalid/ticket/12008): Fixed various cases of inserting a single non-editable element using the [`editor.insertHtml()`](http://example.invalid/#!/api/CKEDITOR.editor-method-insertHtml) method. Fixes pasting a widget with a nested editable inside another widget's nested editable.

* Notification system:
  * [#11580](http://example.invalid/ticket/11580): Introduced the [notification system](http://example.invalid/#!/api/CKEDITOR.plugins.notification).
  * [#12810](http://example.invalid/ticket/12810): Introduced a [notification aggregator](http://example.invalid/#!/api/CKEDITOR.plugins.notificationAggregator) for the [notification system](http://example.invalid/#!/api/CKEDITOR.plugins.notification) which simplifies displaying progress of many concurrent tasks.
* [#11636](http://example.invalid/ticket/11636): Introduced new, UX-focused, methods for getting selected HTML and deleting it &mdash; [`editor.getSelectedHtml()`](http://example.invalid/#!/api/CKEDITOR.editor-method-getSelectedHtml) and [`editor.deleteSelectedHtml()`](http://example.invalid/#!/api/CKEDITOR.editor-method-getSelectedHtml).
* [#12416](http://example.invalid/ticket/12416): Added the [`widget.definition.upcastPriority`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.definition-property-upcastPriority) property which gives more control over widget upcasting order to the widget author.
* [#12036](http://example.invalid/ticket/12036): Initialize the editor in [read-only](http://example.invalid/#!/api/CKEDITOR.editor-property-readOnly) mode when the `<textarea>` element has a `readonly` attribute.
* [#11905](http://example.invalid/ticket/11905): The [`resize` event](http://example.invalid/#!/api/CKEDITOR.editor-event-resize) passes the current dimensions in its data.
* [#12126](http://example.invalid/ticket/12126): Introduced [`config.image_prefillDimensions`](http://example.invalid/#!/api/CKEDITOR.config-cfg-image_prefillDimensions) and [`config.image2_prefillDimensions`](http://example.invalid/#!/api/CKEDITOR.config-cfg-image2_prefillDimensions) to make pre-filling `width` and `height` configurable for the [Enhanced Image](http://example.invalid/addon/image2).
* [#12746](http://example.invalid/ticket/12746): Added a new configuration option to hide the [Enhanced Image](http://example.invalid/addon/image2) resizer.
* [#12150](http://example.invalid/ticket/12150): Exposed the [`getNestedEditable()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget-static-method-getNestedEditable) and `is*` [widget helper](http://example.invalid/#!/api/CKEDITOR.plugins.widget) functions (see the static methods).
* [#12448](http://example.invalid/ticket/12448): Introduced the [`editable.insertHtmlIntoRange`](http://example.invalid/#!/api/CKEDITOR.editable-method-insertHtmlIntoRange) method.
* [#12143](http://example.invalid/ticket/12143): Added the [`config.floatSpacePreferRight`](http://example.invalid/#!/api/CKEDITOR.config-cfg-floatSpacePreferRight) configuration option that switches the alignment of the floating toolbar. Thanks to [InvisibleBacon](http://example.invalid/InvisibleBacon)!
* [#10986](http://example.invalid/ticket/10986): Added support for changing dialog input and textarea text directions by using the *Shift+Alt+Home/End* keystrokes. The direction is stored in the value of the input by prepending the [`\u202A`](http://example.invalid/cldr/utility/character.jsp?a=202A) or [`\u202B`](http://example.invalid/cldr/utility/character.jsp?a=202B) marker to it. Read more in the [documentation](http://example.invalid/#!/api/CKEDITOR.dialog.definition.textInput-property-bidi). Thanks to [edithkk](https://example.invalid/edithkk)!
* [#12770](http://example.invalid/ticket/12770): Added support for passing [widget](http://example.invalid/#!/api/CKEDITOR.plugins.widget)'s startup data as a widget command's argument. Thanks to [Rebrov Boris](https://example.invalid/zipp3r) and [Tieme van Veen](https://example.invalid/tiemevanveen)!
* [#11583](http://example.invalid/ticket/11583): Added support for the HTML5 `required` attribute in various form elements. Thanks to [Steven Busse](https://example.invalid/sbusse)!

Changes:

* [#12858](http://example.invalid/ticket/12858): Basic [Spartan](http://example.invalid/bloggingwindows/2015/03/30/introducing-project-spartan-the-new-browser-built-for-windows-10/) browser compatibility. Full compatibility will be introduced later, because at the moment Spartan is still too unstable to be used for tests and we see many changes from version to version.
* [#12948](http://example.invalid/ticket/12948): The [`config.mathJaxLibrary`](http://example.invalid/#!/api/CKEDITOR.config-cfg-mathJaxLib) option does not default to the MathJax CDN any more. It needs to be configured to enable the [Mathematical Formulas](http://example.invalid/addon/mathjax) plugin now.
* [#13069](http://example.invalid/ticket/13069): Fixed inconsistencies between [`editable.insertHtml()`](http://example.invalid/#!/api/CKEDITOR.editable-method-insertElement) and [`editable.insertElement()`](http://example.invalid/#!/api/CKEDITOR.editable-method-insertElement) when the `range` parameter is used. Now, the `editor.insertElement()` method works on a higher level, which means that it saves undo snapshots and sets the selection after insertion. Use the [`editable.insertElementIntoRange()`](http://example.invalid/#!/api/CKEDITOR.editable-method-insertElementIntoRange) method directly for the pre 4.5 behavior of `editable.insertElement()`.
* [#12870](http://example.invalid/ticket/12870): Use [`editor.showNotification()`](http://example.invalid/#!/api/CKEDITOR.editor-method-showNotification) instead of `alert()` directly whenever possible. When the [Notification plugin](http://example.invalid/addon/notification) is loaded, the notification system is used automatically. Otherwise, the native `alert()` is displayed.
* [#8024](http://example.invalid/ticket/8024): Swapped behavior of the Split Cell Vertically and Horizontally features of the [Table Tools](http://example.invalid/addon/tabletools) plugin to be more intuitive. Thanks to [kevinisagit](https://example.invalid/kevinisagit)!
* [#10903](http://example.invalid/ticket/10903): Performance improvements for the [`dom.element.addClass()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-addClass), [`dom.element.removeClass()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-removeClass) and [`dom.element.hasClass()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-hasClass) methods. Note: The previous implementation allowed passing multiple classes to `addClass()` although it was only a side effect of that implementation. The new implementation does not allow this.
* [#11856](http://example.invalid/ticket/11856): The jQuery adapter throws a meaningful error if CKEditor or jQuery are not loaded.

Fixed issues:

* [#11586](http://example.invalid/ticket/11586): Fixed: [`range.cloneContents()`](http://example.invalid/#!/api/CKEDITOR.dom.range-method-cloneContents) should not change the DOM in order not to affect selection.
* [#12148](http://example.invalid/ticket/12148): Fixed: [`dom.element.getChild()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-getChild) should not modify a passed array.
* [#12503](http://example.invalid/ticket/12503): [Blink/Webkit] Fixed: Incorrect result of Select All and *Backspace* or *Delete*.
* [#13001](http://example.invalid/ticket/13001): [Firefox] Fixed: The `<br />` filler is placed in the wrong position by the [`range.fixBlock()`](http://example.invalid/#!/api/CKEDITOR.dom.range-method-fixBlock) method due to quirky Firefox behavior.
* [#13101](http://example.invalid/ticket/13101): [IE8] Fixed: Colons are prepended to HTML5 element names when cloning them.

## CKEditor 4.4.8

**Security Updates:**

* Fixed XSS vulnerability in the HTML parser reported by [Dheeraj Joshi](https://example.invalid/dheerajhere) and [Prem Kumar](https://example.invalid/iAmPr3m).

	Issue summary: It was possible to execute XSS inside CKEditor after persuading the victim to: (i) switch CKEditor to source mode, then (ii) paste a specially crafted HTML code, prepared by the attacker, into the opened CKEditor source area, and (iii) switch back to WYSIWYG mode.

**An upgrade is highly recommended!**

Fixed Issues:

* [#12899](http://example.invalid/ticket/12899): Fixed: Corrected wrong tag ending for horizontal box definition in the [Dialog User Interface](http://example.invalid/addon/dialogui) plugin. Thanks to [mizafish](https://example.invalid/mizafish)!
* [#13254](http://example.invalid/ticket/13254): Fixed: Cannot outdent block after indent when using the [Div Editing Area](http://example.invalid/addon/divarea) plugin. Thanks to [Jonathan Cottrill](https://example.invalid/jcttrll)!
* [#13268](http://example.invalid/ticket/13268): Fixed: Documentation for [`CKEDITOR.dom.text`](http://example.invalid/#!/api/CKEDITOR.dom.text) is incorrect. Thanks to [Ben Kiefer](https://example.invalid/benkiefer)!
* [#12739](http://example.invalid/ticket/12739): Fixed: Link loses inline styles when edited without the [Advanced Tab for Dialogs](http://example.invalid/addon/dialogadvtab) plugin. Thanks to [Віталій Крутько](https://example.invalid/asmforce)!
* [#13292](http://example.invalid/ticket/13292): Fixed: Protection pattern does not work in attribute in self-closing elements with no space before `/>`. Thanks to [Віталій Крутько](https://example.invalid/asmforce)!
* [PR#192](https://example.invalid/ckeditor/ckeditor-dev/pull/192): Fixed: Variable name typo in the [Dialog User Interface](http://example.invalid/addon/dialogui) plugin which caused [`CKEDITOR.ui.dialog.radio`](http://example.invalid/#!/api/CKEDITOR.ui.dialog.radio) validation to not work. Thanks to [Florian Ludwig](https://example.invalid/FlorianLudwig)!
* [#13232](http://example.invalid/ticket/13232): [Safari] Fixed: The [`element.appendText()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-appendText) method does not work properly for empty elements.
* [#13233](http://example.invalid/ticket/13233): Fixed: [HTMLDataProcessor](http://example.invalid/#!/api/CKEDITOR.htmlDataProcessor) can process `foo:href` attributes.
* [#12796](http://example.invalid/ticket/12796): Fixed: The [Indent List](http://example.invalid/addon/indentlist) plugin unwraps parent `<li>` elements. Thanks to [Andrew Stucki](https://example.invalid/andrewstucki)!
* [#12885](http://example.invalid/ticket/12885): Added missing [`editor.getData()`](http://example.invalid/#!/api/CKEDITOR.editor-method-getData) parameter documentation.
* [#11982](http://example.invalid/ticket/11982): Fixed: Bullet added in a wrong position after the *Enter* key is pressed in a nested list.
* [#13027](http://example.invalid/ticket/13027): Fixed: Keyboard navigation in dialog windows with multiple tabs not following IBM CI 162 instructions or [ARIA Authoring Practices](http://example.invalid/TR/2013/WD-wai-aria-practices-20130307/#tabpanel).
* [#12256](http://example.invalid/ticket/12256): Fixed: Basic styles classes are lost when pasting from Microsoft Word if [basic styles](http://example.invalid/addon/basicstyles) were configured to use classes.
* [#12729](http://example.invalid/ticket/12729): Fixed: Incorrect structure created when merging a block into a list item on *Backspace* and *Delete*.
* [#13031](http://example.invalid/ticket/13031): [Firefox] Fixed: No more line breaks in source view since Firefox 36.
* [#13131](http://example.invalid/ticket/13131): Fixed: The [Code Snippet](http://example.invalid/addon/codesnippet) plugin cannot be used without the [IFrame Editing Area](http://example.invalid/addon/wysiwygarea) plugin.
* [#9086](http://example.invalid/ticket/9086): Fixed: Invalid ARIA property used on paste area `<iframe>`.
* [#13164](http://example.invalid/ticket/13164): Fixed: Error when inserting a hidden field.
* [#13155](http://example.invalid/ticket/13155): Fixed: Incorrect [Line Utilities](http://example.invalid/addon/lineutils) positioning when `<body>` has a margin.
* [#13351](http://example.invalid/ticket/13351): Fixed: Link lost when editing a linked image with the Link tab disabled. This also fixed a bug when inserting an image into a fully selected link would throw an error ([#12847](https://example.invalid/ticket/12847)).
* [#13344](http://example.invalid/ticket/13344): [WebKit/Blink] Fixed: It is possible to remove or change editor content in [read-only mode](http://example.invalid/#!/guide/dev_readonly).

Other Changes:

* [#12844](http://example.invalid/ticket/12844) and [#13103](http://example.invalid/ticket/13103): Upgraded the [testing environment](http://example.invalid/#!/guide/dev_tests) to [Bender.js](https://example.invalid/benderjs/benderjs) `0.2.3`.
* [#12930](http://example.invalid/ticket/12930): Because of licensing issues, `truncated-mathjax/` is now removed from the `tests/` directory. Now `bender.config.mathJaxLibPath` must be configured manually in order to run [Mathematical Formulas](http://example.invalid/addon/mathjax) plugin tests.
* [#13266](http://example.invalid/ticket/13266): Added more shades of gray in the [Color Dialog](http://example.invalid/addon/colordialog) window. Thanks to [mizafish](https://example.invalid/mizafish)!


## CKEditor 4.4.7

Fixed Issues:

* [#12825](http://example.invalid/ticket/12825): Fixed: Preventing the [Table Resize](http://example.invalid/addon/tableresize) plugin from operating on elements outside the editor. Thanks to [Paul Martin](https://example.invalid/Paul-Martin)!
* [#12157](http://example.invalid/ticket/12157): Fixed: Lost text formatting on pressing *Tab* when the [`config.tabSpaces`](http://example.invalid/#!/api/CKEDITOR.config-cfg-tabSpaces) configuration option value was greater than zero.
* [#12777](http://example.invalid/ticket/12777): Fixed: The `table-layout` CSS property should be reset by skins. Thanks to [vita10gy](https://example.invalid/vita10gy)!
* [#12812](http://example.invalid/ticket/12812): Fixed: An uncaught security exception is thrown when [Line Utilities](http://example.invalid/addon/lineutils) are used in an inline editor loaded in a cross-domain `iframe`. Thanks to [Vitaliy Zurian](https://example.invalid/thecatontheflat)!
* [#12735](http://example.invalid/ticket/12735): Fixed: [`config.fillEmptyBlocks`](http://example.invalid/#!/api/CKEDITOR.config-cfg-fillEmptyBlocks) should only apply when outputting data.
* [#10032](http://example.invalid/ticket/10032): Fixed: [Paste from Word](http://example.invalid/addon/pastefromword) filter is executed for every paste after using the button.
* [#12597](http://example.invalid/ticket/12597): [Blink/WebKit] Fixed: Multi-byte Japanese characters entry not working properly after *Shift+Enter*.
* [#12387](http://example.invalid/ticket/12387): Fixed: An error is thrown if a skin does not have the [`chameleon`](http://example.invalid/#!/api/CKEDITOR.skin-method-chameleon) property defined and [`config.uiColor`](http://example.invalid/#!/api/CKEDITOR.config-cfg-uiColor) is defined.
* [#12747](http://example.invalid/ticket/12747): [IE8-10] Fixed: Opening a drop-down for a specific selection when the editor is maximized results in incorrect drop-down panel position.
* [#12850](http://example.invalid/ticket/12850): [IEQM] Fixed: An error is thrown after focusing the editor.

## CKEditor 4.4.6

**Security Updates:**

* Fixed XSS vulnerability in the HTML parser reported by [Maco Cortes](https://example.invalid/Maaacoooo).

	Issue summary: It was possible to execute XSS inside CKEditor after persuading the victim to: (i) switch CKEditor to source mode, then (ii) paste a specially crafted HTML code, prepared by the attacker, into the opened CKEditor source area, and (iii) switch back to WYSIWYG mode.

**An upgrade is highly recommended!**

New Features:

* [#12501](http://example.invalid/ticket/12501): Allowed dashes in element names in the [string format of allowed content rules](http://example.invalid/#!/guide/dev_allowed_content_rules-section-string-format).
* [#12550](http://example.invalid/ticket/12550): Added the `<main>` element to the [`CKEDITOR.dtd`](http://example.invalid/#!/api/CKEDITOR.dtd).

Fixed Issues:

* [#12506](http://example.invalid/ticket/12506): [Safari] Fixed: Cannot paste into inline editor if the page has `user-select: none` style. Thanks to [shaohua](https://example.invalid/shaohua)!
* [#12683](http://example.invalid/ticket/12683): Fixed: [Filter](http://example.invalid/#!/guide/dev_acf) fails to remove custom tags. Thanks to [timselier](https://example.invalid/timselier)!
* [#12489](http://example.invalid/ticket/12489) and [#12491](http://example.invalid/ticket/12491): Fixed: Various issues related to restoring the selection after performing operations on filler character. See the [fixed cases](http://example.invalid/ticket/12491#comment:4).
* [#12621](http://example.invalid/ticket/12621): Fixed: Cannot remove inline styles (bold, italic, etc.) in empty lines.
* [#12630](http://example.invalid/ticket/12630): [Chrome] Fixed: Selection is placed outside the paragraph when the [New Page](http://example.invalid/addon/newpage) button is clicked. This patch significantly simplified the way how the initial selection (a selection after the content of the editable is overwritten) is being fixed. That might have fixed many related scenarios in all browsers.
* [#11647](http://example.invalid/ticket/11647): Fixed: The [`editor.blur`](http://example.invalid/#!/api/CKEDITOR.editor-event-blur) event is not fired on first blur after initializing the inline editor on an already focused element.
* [#12601](http://example.invalid/ticket/12601): Fixed: [Strikethrough](http://example.invalid/addon/basicstyles) button tooltip spelling.
* [#12546](http://example.invalid/ticket/12546): Fixed: The Preview tab in the [Document Properties](http://example.invalid/addon/docprops) dialog window is always disabled.
* [#12300](http://example.invalid/ticket/12300): Fixed: The [`editor.change`](http://example.invalid/#!/api/CKEDITOR.editor-event-change) event fired on first navigation key press after typing.
* [#12141](http://example.invalid/ticket/12141): Fixed: List items are lost when indenting a list item with content wrapped with a block element.
* [#12515](http://example.invalid/ticket/12515): Fixed: Cursor is in the wrong position when undoing after adding an image and typing some text.
* [#12484](http://example.invalid/ticket/12484): [Blink/WebKit] Fixed: DOM is changed outside the editor area in a certain case.
* [#12688](http://example.invalid/ticket/12688): Improved the tests of the [styles system](http://example.invalid/#!/api/CKEDITOR.style) and fixed two minor issues.
* [#12403](http://example.invalid/ticket/12403): Fixed: Changing the [font](http://example.invalid/addon/font) style should not lead to nesting it in the previous style element.
* [#12609](http://example.invalid/ticket/12609): Fixed: Incorrect `config.magicline_putEverywhere` name used for a [Magic Line](http://example.invalid/addon/magicline) all-encompassing [`config.magicline_everywhere`](http://example.invalid/#!/api/CKEDITOR.config-cfg-magicline_everywhere) configuration option.


## CKEditor 4.4.5

New Features:

* [#12279](http://example.invalid/ticket/12279): Added a possibility to pass a custom evaluator to [`node.getAscendant()`](http://example.invalid/#!/api/CKEDITOR.dom.node-method-getAscendant).

Fixed Issues:

* [#12423](http://example.invalid/ticket/12423): [Safari7.1+] Fixed: *Enter* key moved cursor to a strange position.
* [#12381](http://example.invalid/ticket/12381): [iOS] Fixed: Selection issue. Thanks to [Remiremi](https://example.invalid/Remiremi)!
* [#10804](http://example.invalid/ticket/10804): Fixed: `CKEDITOR_GETURL` is not used with some plugins where it should be used. Thanks to [Thomas Andraschko](https://example.invalid/tandraschko)!
* [#9137](http://example.invalid/ticket/9137): Fixed: The `<base>` tag is not created when `<head>` has an attribute. Thanks to [naoki.fujikawa](https://example.invalid/naoki-fujikawa)!
* [#12377](http://example.invalid/ticket/12377): Fixed: Errors thrown in the [Image](http://example.invalid/addon/image) plugin when removing preview from the dialog window definition. Thanks to [Axinet](https://example.invalid/Axinet)!
* [#12162](http://example.invalid/ticket/12162): Fixed: Auto paragraphing and *Enter* key in nested editables.
* [#12315](http://example.invalid/ticket/12315): Fixed: Marked [`config.autoParagraph`](http://example.invalid/#!/api/CKEDITOR.config-cfg-autoParagraph) as deprecated.
* [#12113](http://example.invalid/ticket/12113): Fixed: A [code snippet](http://example.invalid/addon/codesnippet) should be presented in the [elements path](http://example.invalid/addon/elementspath) as "code snippet" (translatable).
* [#12311](http://example.invalid/ticket/12311): Fixed: [Remove Format](http://example.invalid/addon/removeformat) should also remove `<cite>` elements.
* [#12261](http://example.invalid/ticket/12261): Fixed: Filter has to be destroyed and removed from [`CKEDITOR.filter.instances`](http://example.invalid/#!/api/CKEDITOR.filter-static-property-instances) on editor destroy.
* [#12398](http://example.invalid/ticket/12398): Fixed: [Maximize](http://example.invalid/addon/maximize) does not work on an instance without a [title](http://example.invalid/#!/api/CKEDITOR.config-cfg-title).
* [#12097](http://example.invalid/ticket/12097): Fixed: JAWS not reading the number of options correctly in the [Text Color and Background Color](http://example.invalid/addon/colorbutton) button menu.
* [#12411](http://example.invalid/ticket/12411): Fixed: [Page Break](http://example.invalid/addon/pagebreak) used directly in the editable breaks the editor.
* [#12354](http://example.invalid/ticket/12354): Fixed: Various issues in undo manager when holding keys.
* [#12324](http://example.invalid/ticket/12324): [IE8] Fixed: Undo steps are not recorded when changing the caret position by clicking below the body.
* [#12332](http://example.invalid/ticket/12332): Fixed: Lowered DOM events listeners' priorities in undo manager in order to avoid ambiguity.
* [#12402](http://example.invalid/ticket/12402): [Blink] Fixed: Workaround for Blink bug with `document.title` which breaks updating title in the full HTML mode.
* [#12338](http://example.invalid/ticket/12338): Fixed: The CKEditor package contains unoptimized images.


## CKEditor 4.4.4

Fixed Issues:

* [#12268](http://example.invalid/ticket/12268): Cleanup of [UI Color](http://example.invalid/addon/uicolor) YUI styles. Thanks to [CasherWest](https://example.invalid/CasherWest)!
* [#12263](http://example.invalid/ticket/12263): Fixed: [Paste from Word](http://example.invalid/addon/pastefromword) filter does not properly normalize semicolons style text. Thanks to [Alin Purcaru](https://example.invalid/mesmerizero)!
* [#12243](http://example.invalid/ticket/12243): Fixed: Text formatting lost when pasting from Word. Thanks to [Alin Purcaru](https://example.invalid/mesmerizero)!
* [#111739](http://example.invalid/ticket/11739): Fixed: `keypress` listeners should not be used in the undo manager. A complete rewrite of keyboard handling in the undo manager was made. Numerous smaller issues were fixed, among others:
  * [#10926](http://example.invalid/ticket/10926): [Chrome@Android] Fixed: Typing does not record snapshots and does not fire the [`editor.change`](http://example.invalid/#!/api/CKEDITOR.editor-event-change) event.
  * [#11611](http://example.invalid/ticket/11611): [Firefox] Fixed: The [`editor.change`](http://example.invalid/#!/api/CKEDITOR.editor-event-change) event is fired when pressing Arrow keys.
  * [#12219](http://example.invalid/ticket/12219): [Safari] Fixed: Some modifications of the [`UndoManager.locked`](http://example.invalid/#!/api/CKEDITOR.plugins.undo.UndoManager-property-locked) property violate strict mode in the [Undo](http://example.invalid/addon/undo) plugin.
* [#10916](http://example.invalid/ticket/10916): Fixed: [Magic Line](http://example.invalid/addon/magicline) icon in Right-To-Left environments.
* [#11970](http://example.invalid/ticket/11970): [IE] Fixed: CKEditor `paste` event is not fired when pasting with *Shift+Ins*.
* [#12111](http://example.invalid/ticket/12111): Fixed: Linked image attributes are not read when opening the image dialog window by doubleclicking.
* [#10030](http://example.invalid/ticket/10030): [IE] Fixed: Prevented "Unspecified Error" thrown in various cases when IE8-9 does not allow access to `document.activeElement`.
* [#12273](http://example.invalid/ticket/12273): Fixed: Applying block style in a description list breaks it.
* [#12218](http://example.invalid/ticket/12218): Fixed: Minor syntax issue in CSS files.
* [#12178](http://example.invalid/ticket/12178): [Blink/WebKit] Fixed: Iterator does not return the block if the selection is located at the end of it.
* [#12185](http://example.invalid/ticket/12185): [IE9QM] Fixed: Error thrown when moving the mouse over focused editor's scrollbar.
* [#12215](http://example.invalid/ticket/12215): Fixed: Basepath resolution does not recognize semicolon as a query separator.
* [#12135](http://example.invalid/ticket/12135): Fixed: [Remove Format](http://example.invalid/addon/removeformat) does not work on widgets.
* [#12298](http://example.invalid/ticket/12298): [IE11] Fixed: Clicking below `<body>` in Compatibility Mode will no longer reset selection to the first line.
* [#12204](http://example.invalid/ticket/12204): Fixed: Editor's voice label is not affected by [`config.title`](http://example.invalid/#!/api/CKEDITOR.config-cfg-title).
* [#11915](http://example.invalid/ticket/11915): Fixed: With [SCAYT](http://example.invalid/addon/scayt) enabled, cursor moves to the beginning of the first highlighted, misspelled word after typing or pasting into the editor.
* [SCAYT](https://example.invalid/WebSpellChecker/ckeditor-plugin-scayt/issues/69): Fixed: Error thrown in the console after enabling [SCAYT](http://example.invalid/addon/scayt) and trying to add a new image.


Other Changes:

* [#12296](http://example.invalid/ticket/12296): Merged `benderjs-ckeditor` into the main CKEditor repository.

## CKEditor 4.4.3

**Security Updates:**

* Fixed XSS vulnerability in the Preview plugin reported by Mario Heiderich of [Cure53](https://cure53.de/).

**An upgrade is highly recommended!**

New Features:

* [#12164](http://example.invalid/ticket/12164): Added the "Justify" option to the "Horizontal Alignment" drop-down in the Table Cell Properties dialog window.

Fixed Issues:

* [#12110](http://example.invalid/ticket/12110): Fixed: Editor crash after deleting a table. Thanks to [Alin Purcaru](https://example.invalid/mesmerizero)!
* [#11897](http://example.invalid/ticket/11897): Fixed: *Enter* key used in an empty list item creates a new line instead of breaking the list. Thanks to [noam-si](https://example.invalid/noam-si)!
* [#12140](http://example.invalid/ticket/12140): Fixed: Double-clicking linked widgets opens two dialog windows.
* [#12132](http://example.invalid/ticket/12132): Fixed: Image is inserted with `width` and `height` styles even when they are not allowed.
* [#9317](http://example.invalid/ticket/9317): [IE] Fixed: [`config.disableObjectResizing`](http://example.invalid/#!/api/CKEDITOR.config-cfg-disableObjectResizing) does not work on IE. **Note**: We were not able to fix this issue on IE11+ because necessary events stopped working. See a [last resort workaround](http://example.invalid/ticket/9317#comment:16) and make sure to [support our complaint to Microsoft](https://example.invalid/IE/feedback/details/742593/please-respect-execcommand-enableobjectresizing-in-contenteditable-elements).
* [#9638](http://example.invalid/ticket/9638): Fixed: There should be no information about accessibility help available under the *Alt+0* keyboard shortcut if the [Accessibility Help](http://example.invalid/addon/a11yhelp) plugin is not available.
* [#8117](http://example.invalid/ticket/8117) and [#9186](http://example.invalid/ticket/9186): Fixed: In HTML5 `<meta>` tags should be allowed everywhere, including inside the `<body>` element.
* [#10422](http://example.invalid/ticket/10422): Fixed: [`config.fillEmptyBlocks`](http://example.invalid/#!/api/CKEDITOR.config-cfg-fillEmptyBlocks) not working properly if a function is specified.

## CKEditor 4.4.2

Important Notes:

* The CKEditor testing environment is now publicly available. Read more about how to set up the environment and execute tests in the [CKEditor Testing Environment](http://example.invalid/#!/guide/dev_tests) guide.
	Please note that the [`tests/`](https://example.invalid/ckeditor/ckeditor-dev/tree/master/tests) directory which contains editor tests is not available in release packages. It can only be found in the development version of CKEditor on [GitHub](https://example.invalid/ckeditor/ckeditor-dev/).

New Features:

* [#11909](http://example.invalid/ticket/11909): Introduced a parameter to prevent the [`editor.setData()`](http://example.invalid/#!/api/CKEDITOR.editor-method-setData) method from recording undo snapshots.

Fixed Issues:

* [#11757](http://example.invalid/ticket/11757): Fixed: Imperfections in the [Moono](http://example.invalid/addon/moono) skin. Thanks to [danyaPostfactum](https://example.invalid/danyaPostfactum)!
* [#10091](http://example.invalid/ticket/10091): Blockquote should be treated like an object by the styles system. Thanks to [dan-james-deeson](https://example.invalid/dan-james-deeson)!
* [#11478](http://example.invalid/ticket/11478): Fixed: Issue with passing jQuery objects to [adapter](http://example.invalid/#!/guide/dev_jquery) configuration.
* [#10867](http://example.invalid/ticket/10867): Fixed: Issue with setting encoded URI as image link.
* [#11983](http://example.invalid/ticket/11983): Fixed: Clicking a nested widget does not focus it. Additionally, performance of the [`widget.repository.getByElement()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.repository-method-getByElement) method was improved.
* [#12000](http://example.invalid/ticket/12000): Fixed: Nested widgets should be initialized on [`editor.setData()`](http://example.invalid/#!/api/CKEDITOR.editor-method-setData) and [`nestedEditable.setData()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.nestedEditable-method-setData).
* [#12022](http://example.invalid/ticket/12022): Fixed: Outer widget's drag handler is not created at all if it has any nested widgets inside.
* [#11960](http://example.invalid/ticket/11960): [Blink/WebKit] Fixed: The caret should be scrolled into view on *Backspace* and *Delete* (covers only the merging blocks case).
* [#11306](http://example.invalid/ticket/11306): [OSX][Blink/WebKit] Fixed: No widget entries in the context menu on widget right-click.
* [#11957](http://example.invalid/ticket/11957): Fixed: Alignment labels in the [Enhanced Image](http://example.invalid/addon/image2) dialog window are not translated.
* [#11980](http://example.invalid/ticket/11980): [Blink/WebKit] Fixed: `<span>` elements created when joining adjacent elements (non-collapsed selection).
* [#12009](http://example.invalid/ticket/12009): [Nested widgets] Integration with the [Magic Line](http://example.invalid/addon/magicline) plugin.
* [#11387](http://example.invalid/ticket/11387): Fixed: `role="radiogroup"` should be applied only to radio inputs' container.
* [#7975](http://example.invalid/ticket/7975): [IE8] Fixed: Errors when trying to select an empty table cell.
* [#11947](http://example.invalid/ticket/11947): [Firefox+IE11] Fixed: *Shift+Enter* in lists produces two line breaks.
* [#11972](http://example.invalid/ticket/11972): Fixed: Feature detection in the [`element.setText()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-setText) method should not trigger the layout engine.
* [#7634](http://example.invalid/ticket/7634): Fixed: The [Flash Dialog](http://example.invalid/addon/flash) plugin omits the `allowFullScreen` parameter in the editor data if set to `true`.
* [#11910](http://example.invalid/ticket/11910): Fixed: [Enhanced Image](http://example.invalid/addon/image2) does not take [`config.baseHref`](http://example.invalid/#!/api/CKEDITOR.config-cfg-baseHref) into account when updating image dimensions.
* [#11753](http://example.invalid/ticket/11753): Fixed: Wrong [`checkDirty()`](http://example.invalid/#!/api/CKEDITOR.editor-method-checkDirty) method value after focusing or blurring a widget.
* [#11830](http://example.invalid/ticket/11830): Fixed: Impossible to pass some arguments to [CKBuilder](https://example.invalid/ckeditor/ckbuilder) when using the `/dev/builder/build.sh` script.
* [#11945](http://example.invalid/ticket/11945): Fixed: [Form Elements](http://example.invalid/addon/forms) plugin should not change a core method.
* [#11384](http://example.invalid/ticket/11384): [IE9+] Fixed: `IndexSizeError` thrown when pasting into a non-empty selection anchored in one text node.

## CKEditor 4.4.1

New Features:

* [#9661](http://example.invalid/ticket/9661): Added the option to [configure](http://example.invalid/#!/api/CKEDITOR.config-cfg-linkJavaScriptLinksAllowed) anchor tags with JavaScript code in the `href` attribute.

Fixed Issues:

* [#11861](http://example.invalid/ticket/11861): [WebKit/Blink] Fixed: Span elements created while joining adjacent elements. **Note:** This patch only covers cases when *Backspace* or *Delete* is pressed on a collapsed (empty) selection. The remaining case, with a non-empty selection, will be fixed in the next release.
* [#10714](http://example.invalid/ticket/10714): [iOS] Fixed: Selection and drop-downs are broken if a touch event listener is used due to a [WebKit bug](https://example.invalid/show_bug.cgi?id=128924). Thanks to [Arty Gus](https://example.invalid/artygus)!
* [#11911](http://example.invalid/ticket/11911): Fixed setting the `dir` attribute for a preloaded language in [CKEDITOR.lang](http://example.invalid/#!/api/CKEDITOR.lang). Thanks to [Akash Mohapatra](https://example.invalid/akashmohapatra)!
* [#11926](http://example.invalid/ticket/11926): Fixed: [Code Snippet](http://example.invalid/addon/codesnippet) does not decode HTML entities when loading code from the `<code>` element.
* [#11223](http://example.invalid/ticket/11223): Fixed: Issue when [Protected Source](http://example.invalid/#!/api/CKEDITOR.config-cfg-protectedSource) was not working in the `<title>` element.
* [#11859](http://example.invalid/ticket/11859): Fixed: Removed the [Source Dialog](http://example.invalid/addon/sourcedialog) plugin dependency from the [Code Snippet](http://example.invalid/addon/codesnippet) sample.
* [#11754](http://example.invalid/ticket/11754): [Chrome] Fixed: Infinite loop when content includes not closed attributes.
* [#11848](http://example.invalid/ticket/11848): [IE] Fixed: [`editor.insertElement()`](http://example.invalid/#!/api/CKEDITOR.editor-method-insertElement) throwing an exception when there was no selection in the editor.
* [#11801](http://example.invalid/ticket/11801): Fixed: Editor anchors unavailable when linking the [Enhanced Image](http://example.invalid/addon/image2) widget.
* [#11626](http://example.invalid/ticket/11626): Fixed: [Table Resize](http://example.invalid/addon/tableresize) sets invalid column width.
* [#11872](http://example.invalid/ticket/11872): Made [`element.addClass()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-addClass) chainable symmetrically to [`element.removeClass()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-removeClass).
* [#11813](http://example.invalid/ticket/11813): Fixed: Link lost while pasting a captioned image and restoring an undo snapshot ([Enhanced Image](http://example.invalid/addon/image2)).
* [#11814](http://example.invalid/ticket/11814): Fixed: _Link_ and _Unlink_ entries persistently displayed in the [Enhanced Image](http://example.invalid/addon/image2) context menu.
* [#11839](http://example.invalid/ticket/11839): [IE9] Fixed: The caret jumps out of the editable area when resizing the editor in the source mode.
* [#11822](http://example.invalid/ticket/11822): [WebKit] Fixed: Editing anchors by double-click is broken in some cases.
* [#11823](http://example.invalid/ticket/11823): [IE8] Fixed: [Table Resize](http://example.invalid/addon/tableresize) throws an error over scrollbar.
* [#11788](http://example.invalid/ticket/11788): Fixed: It is not possible to change the language back to _Not set_ in the [Code Snippet](http://example.invalid/addon/codesnippet) dialog window.
* [#11788](http://example.invalid/ticket/11788): Fixed: [Filter](http://example.invalid/#!/api/CKEDITOR.htmlParser.filter) rules are not applied inside elements with the `contenteditable` attribute set to `true`.
* [#11798](http://example.invalid/ticket/11798): Fixed: Inserting a non-editable element inside a table cell breaks the table.
* [#11793](http://example.invalid/ticket/11793): Fixed: Drop-down is not "on" when clicking it while the editor is blurred.
* [#11850](http://example.invalid/ticket/11850): Fixed: Fake objects with the `contenteditable` attribute set to `false` are not downcasted properly.
* [#11811](http://example.invalid/ticket/11811): Fixed: Widget's data is not encoded correctly when passed to an attribute.
* [#11777](http://example.invalid/ticket/11777): Fixed encoding ampersand in the [Mathematical Formulas](http://example.invalid/addon/mathjax) plugin.
* [#11880](http://example.invalid/ticket/11880): [IE8-9] Fixed: Linked image has a default thick border.

Other Changes:

* [#11807](http://example.invalid/ticket/11807): Updated jQuery version used in the sample to 1.11.0 and tested CKEditor jQuery Adapter with version 1.11.0 and 2.1.0.
* [#9504](http://example.invalid/ticket/9504): Stopped using deprecated `attribute.specified` in all browsers except Internet Explorer.
* [#11809](http://example.invalid/ticket/11809): Changed tab size in `<pre>` to 4 spaces.

## CKEditor 4.4

**Important Notes:**

* Marked the [`editor.beforePaste`](http://example.invalid/#!/api/CKEDITOR.editor-event-beforePaste) event as deprecated.
* The default class of captioned images has changed to `image` (was: `caption`). Please note that once edited in CKEditor 4.4+, all existing images of the `caption` class (`<figure class="caption">`) will be [filtered out](http://example.invalid/#!/guide/dev_advanced_content_filter) unless the [`config.image2_captionedClass`](http://example.invalid/#!/api/CKEDITOR.config-cfg-image2_captionedClass) option is set to `caption`. For backward compatibility (i.e. when upgrading), it is highly recommended to use this setting, which also helps prevent CSS conflicts, etc. This does not apply to new CKEditor integrations.
* Widgets without defined buttons are no longer registered automatically to the [Advanced Content Filter](http://example.invalid/#!/guide/dev_advanced_content_filter). Before CKEditor 4.4 widgets were registered to the ACF which was an incorrect behavior ([#11567](http://example.invalid/ticket/11567)). This change should not have any impact on standard scenarios, but if your button does not execute the widget command, you need to set [`allowedContent`](http://example.invalid/#!/api/CKEDITOR.feature-property-allowedContent) and [`requiredContent`](http://example.invalid/#!/api/CKEDITOR.feature-property-requiredContent) properties for it manually, because the editor will not be able to find them.
* The [Show Borders](http://example.invalid/addon/showborders) plugin was added to the Standard installation package in order to ensure that unstyled tables are still visible for the user ([#11665](http://example.invalid/ticket/11665)).
* Since CKEditor 4.4 the editor instance should be passed to [`CKEDITOR.style`](http://example.invalid/#!/api/CKEDITOR.style) methods to ensure full compatibility with other features (e.g. applying styles to widgets requires that). We ensured backward compatibility though, so the [`CKEDITOR.style`](http://example.invalid/#!/api/CKEDITOR.style) will work even when the editor instance is not provided.

New Features:

* [#11297](http://example.invalid/ticket/11297): Styles can now be applied to widgets. The definition of a style which can be applied to a specific widget must contain two additional properties &mdash; `type` and `widget`. Read more in the [Widget Styles](http://example.invalid/#!/guide/dev_styles-section-widget-styles) section of the "Syles Drop-down" guide. Note that by default, widgets support only classes and no other attributes or styles. Related changes and features:
  * Introduced the [`CKEDITOR.style.addCustomHandler()`](http://example.invalid/#!/api/CKEDITOR.style-static-method-addCustomHandler) method for registering custom style handlers.
  * The [`CKEDITOR.style.apply()`](http://example.invalid/#!/api/CKEDITOR.style-method-apply) and [`CKEDITOR.style.remove()`](http://example.invalid/#!/api/CKEDITOR.style-method-remove) methods are now called with an editor instance instead of the document so they can be reused by the [`CKEDITOR.editor.applyStyle()`](http://example.invalid/#!/api/CKEDITOR.editor-method-applyStyle) and [`CKEDITOR.editor.removeStyle()`](http://example.invalid/#!/api/CKEDITOR.editor-method-removeStyle) methods. Backward compatibility was preserved, but from CKEditor 4.4 it is highly recommended to pass an editor instead of a document to these methods.
  * Many new methods and properties were introduced in the [Widget API](http://example.invalid/#!/api/CKEDITOR.plugins.widget) to make the handling of styles by widgets fully customizable. See: [`widget.definition.styleableElements`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.definition-property-styleableElements), [`widget.definition.styleToAllowedContentRule`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.definition-property-styleToAllowedContentRules), [`widget.addClass()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget-method-addClass), [`widget.removeClass()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget-method-removeClass), [`widget.getClasses()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget-method-getClasses), [`widget.hasClass()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget-method-hasClass), [`widget.applyStyle()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget-method-applyStyle), [`widget.removeStyle()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget-method-removeStyle), [`widget.checkStyleActive()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget-method-checkStyleActive).
  * Integration with the [Allowed Content Filter](http://example.invalid/#!/guide/dev_advanced_content_filter) required an introduction of the [`CKEDITOR.style.toAllowedContent()`](http://example.invalid/#!/api/CKEDITOR.style-method-toAllowedContentRules) method which can be implemented by the custom style handler and if exists, it is used by the [`CKEDITOR.filter`](http://example.invalid/#!/api/CKEDITOR.filter) to translate a style to [allowed content rules](http://example.invalid/#!/api/CKEDITOR.filter.allowedContentRules).
* [#11300](http://example.invalid/ticket/11300): Various changes in the [Enhanced Image](http://example.invalid/addon/image2) plugin:
  * Introduced the [`config.image2_captionedClass`](http://example.invalid/#!/api/CKEDITOR.config-cfg-image2_captionedClass) option to configure the class of captioned images.
  * Introduced the [`config.image2_alignClasses`](http://example.invalid/#!/api/CKEDITOR.config-cfg-image2_alignClasses) option to configure the way images are aligned with CSS classes.
  If this setting is defined, the editor produces classes instead of inline styles for aligned images.
  * Default image caption can be translated (customized) with the `editor.lang.image2.captionPlaceholder` string.
* [#11341](http://example.invalid/ticket/11341): [Enhanced Image](http://example.invalid/addon/image2) plugin: It is now possible to add a link to any image type.
* [#10202](http://example.invalid/ticket/10202): Introduced wildcard support in the [Allowed Content Rules](http://example.invalid/#!/guide/dev_allowed_content_rules) format.
* [#10276](http://example.invalid/ticket/10276): Introduced blacklisting in the [Allowed Content Filter](http://example.invalid/#!/guide/dev_advanced_content_filter).
* [#10480](http://example.invalid/ticket/10480): Introduced code snippets with code highlighting. There are two versions available so far &mdash; the default [Code Snippet](http://example.invalid/addon/codesnippet) which uses the [highlight.js](http://example.invalid) library and the [Code Snippet GeSHi](http://example.invalid/addon/codesnippetgeshi) which uses the [GeSHi](http://example.invalid/highlighter/) library.
* [#11737](http://example.invalid/ticket/11737): Introduced an option to prevent [filtering](http://example.invalid/#!/guide/dev_advanced_content_filter) of an element that matches custom criteria (see [`filter.addElementCallback()`](http://example.invalid/#!/api/CKEDITOR.filter-method-addElementCallback)).
* [#11532](http://example.invalid/ticket/11532): Introduced the [`editor.addContentsCss()`](http://example.invalid/#!/api/CKEDITOR.editor-method-addContentsCss) method that can be used for [adding custom CSS files](http://example.invalid/#!/guide/plugin_sdk_styles).
* [#11536](http://example.invalid/ticket/11536): Added the [`CKEDITOR.tools.htmlDecode()`](http://example.invalid/#!/api/CKEDITOR.tools-method-htmlDecode) method for decoding HTML entities.
* [#11225](http://example.invalid/ticket/11225): Introduced the [`CKEDITOR.tools.transparentImageData`](http://example.invalid/#!/api/CKEDITOR.tools-property-transparentImageData) property which contains transparent image data to be used in CSS or as image source.

Other Changes:

* [#11377](http://example.invalid/ticket/11377): Unified internal representation of empty anchors using the [fake objects](http://example.invalid/addon/fakeobjects).
* [#11422](http://example.invalid/ticket/11422): Removed Firefox 3.x, Internet Explorer 6 and Opera 12.x leftovers in code.
* [#5217](http://example.invalid/ticket/5217): Setting data (including switching between modes) creates a new undo snapshot. Besides that:
  * Introduced the [`editable.status`](http://example.invalid/#!/api/CKEDITOR.editable-property-status) property.
  * Introduced a new `forceUpdate` option for the [`editor.lockSnapshot`](http://example.invalid/#!/api/CKEDITOR.editor-event-lockSnapshot) event.
  * Fixed: Selection not being unlocked in inline editor after setting data ([#11500](http://example.invalid/ticket/11500)).
* The [WebSpellChecker](http://example.invalid/addon/wsc) plugin was updated to the latest version.

Fixed Issues:

* [#10190](http://example.invalid/ticket/10190): Fixed: Removing block style with [`editor.removeStyle()`](http://example.invalid/#!/api/CKEDITOR.editor-method-removeStyle) should result in a paragraph and not a div.
* [#11727](http://example.invalid/ticket/11727): Fixed: The editor tries to select a non-editable image which was clicked.

## CKEditor 4.3.5

New Features:

* Added new translation: Tatar.

Fixed Issues:

* [#11677](http://example.invalid/ticket/11677): Fixed: Undo/Redo keystrokes are blocked in the source mode.
* [#11717](http://example.invalid/ticket/11717): [Document Properties](http://example.invalid/addon/docprops) plugin requires the [Color Dialog](http://example.invalid/addon/colordialog) plugin to work.

## CKEditor 4.3.4

Fixed Issues:

* [#11597](http://example.invalid/ticket/11597): [IE11] Fixed: Error thrown when trying to open the [preview](http://example.invalid/addon/preview) using the keyboard.
* [#11544](http://example.invalid/ticket/11544): [Placeholders](http://example.invalid/addon/placeholder) will no longer be upcasted in parents not accepting `<span>` elements.
* [#8663](http://example.invalid/ticket/8663): Fixed [`element.renameNode()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-renameNode) not clearing the [`element.getName()`](http://example.invalid/#!/api/CKEDITOR.dom.element-method-getName) cache.
* [#11574](http://example.invalid/ticket/11574): Fixed: *Backspace* destroying the DOM structure if an inline editable is placed in a list item.
* [#11603](http://example.invalid/ticket/11603): Fixed: [Table Resize](http://example.invalid/addon/tableresize) attaches to tables outside the editable.
* [#9205](http://example.invalid/ticket/9205), [#7805](http://example.invalid/ticket/7805), [#8216](http://example.invalid/ticket/8216): Fixed: `{cke_protected_1}` appearing in data in various cases where HTML comments are placed next to `"` or `'`.
* [#11635](http://example.invalid/ticket/11635): Fixed: Some attributes are not protected before the content is passed through the fix bin.
* [#11660](http://example.invalid/ticket/11660): [IE] Fixed: Table content is lost when some extra markup is inside the table.
* [#11641](http://example.invalid/ticket/11641): Fixed: Switching between modes in the classic editor removes content styles for the inline editor.
* [#11568](http://example.invalid/ticket/11568): Fixed: [Styles](http://example.invalid/addon/stylescombo) drop-down list is not enabled on selection change.

## CKEditor 4.3.3

Fixed Issues:

* [#11500](http://example.invalid/ticket/11500): [WebKit/Blink] Fixed: Selection lost when setting data in another inline editor. Additionally, [`selection.removeAllRanges()`](http://example.invalid/#!/api/CKEDITOR.dom.selection-method-removeAllRanges) is now scoped to selection's [root](http://example.invalid/#!/api/CKEDITOR.dom.selection-property-root).
* [#11104](http://example.invalid/ticket/11104): [IE] Fixed: Various issues with scrolling and selection when focusing widgets.
* [#11487](http://example.invalid/ticket/11487): Moving mouse over the [Enhanced Image](http://example.invalid/addon/image2) widget will no longer change the value returned by the [`editor.checkDirty()`](http://example.invalid/#!/api/CKEDITOR.editor-method-checkDirty) method.
* [#8673](http://example.invalid/ticket/8673): [WebKit] Fixed: Cannot select and remove the [Page Break](http://example.invalid/addon/pagebreak).
* [#11413](http://example.invalid/ticket/11413): Fixed: Incorrect [`editor.execCommand()`](http://example.invalid/#!/api/CKEDITOR.editor-method-execCommand) behavior.
* [#11438](http://example.invalid/ticket/11438): Splitting table cells vertically is no longer changing table structure.
* [#8899](http://example.invalid/ticket/8899): Fixed: Links in the [About CKEditor](http://example.invalid/addon/about) dialog window now open in a new browser window or tab.
* [#11490](http://example.invalid/ticket/11490): Fixed: [Menu button](http://example.invalid/addon/menubutton) panel not showing in the source mode.
* [#11417](http://example.invalid/ticket/11417): The [`widget.doubleclick`](http://example.invalid/#!/api/CKEDITOR.plugins.widget-event-doubleclick) event is not canceled anymore after editing was triggered.
* [#11253](http://example.invalid/ticket/11253): [IE] Fixed: Clipped upload button in the [Enhanced Image](http://example.invalid/addon/image2) dialog window.
* [#11359](http://example.invalid/ticket/11359): Standardized the way anchors are discovered by the [Link](http://example.invalid/addon/link) plugin.
* [#11058](http://example.invalid/ticket/11058): [IE8] Fixed: Error when deleting a table row.
* [#11508](http://example.invalid/ticket/11508): Fixed: [`htmlDataProcessor`](http://example.invalid/#!/api/CKEDITOR.htmlDataProcessor) discovering protected attributes within other attributes' values.
* [#11533](http://example.invalid/ticket/11533): Widgets: Avoid recurring upcasts if the DOM structure was modified during an upcast.
* [#11400](http://example.invalid/ticket/11400): Fixed: The [`domObject.removeAllListeners()`](http://example.invalid/#!/api/CKEDITOR.dom.domObject-method-removeAllListeners) method does not remove custom listeners completely.
* [#11493](http://example.invalid/ticket/11493): Fixed: The [`selection.getRanges()`](http://example.invalid/#!/api/CKEDITOR.dom.selection-method-getRanges) method does not override cached ranges when used with the `onlyEditables` argument.
* [#11390](http://example.invalid/ticket/11390): [IE] All [XML](http://example.invalid/addon/xml) plugin [methods](http://example.invalid/#!/api/CKEDITOR.xml) now work in IE10+.
* [#11542](http://example.invalid/ticket/11542): [IE11] Fixed: Blurry toolbar icons when Right-to-Left UI language is set.
* [#11504](http://example.invalid/ticket/11504): Fixed: When [`config.fullPage`](http://example.invalid/#!/api/CKEDITOR.config-cfg-fullPage) is set to `true`, entities are not encoded in editor output.
* [#11004](http://example.invalid/ticket/11004): Integrated [Enhanced Image](http://example.invalid/addon/image2) dialog window with [Advanced Content Filter](http://example.invalid/#!/guide/dev_advanced_content_filter).
* [#11439](http://example.invalid/ticket/11439): Fixed: Properties get cloned in the Cell Properties dialog window if multiple cells are selected.

## CKEditor 4.3.2

Fixed Issues:

* [#11331](http://example.invalid/ticket/11331): A menu button will have a changed label when selected instead of using the `aria-pressed` attribute.
* [#11177](http://example.invalid/ticket/11177): Widget drag handler improvements:
  * [#11176](http://example.invalid/ticket/11176): Fixed: Initial position is not updated when the widget data object is empty.
  * [#11001](http://example.invalid/ticket/11001): Fixed: Multiple synchronous layout recalculations are caused by initial drag handler positioning causing performance issues.
  * [#11161](http://example.invalid/ticket/11161): Fixed: Drag handler is not repositioned in various situations.
  * [#11281](http://example.invalid/ticket/11281): Fixed: Drag handler and mask are duplicated after widget reinitialization.
* [#11207](http://example.invalid/ticket/11207): [Firefox] Fixed: Misplaced [Enhanced Image](http://example.invalid/addon/image2) resizer in the inline editor.
* [#11102](http://example.invalid/ticket/11102): `CKEDITOR.template` improvements:
  * [#11102](http://example.invalid/ticket/11102): Added newline character support.
  * [#11216](http://example.invalid/ticket/11216): Added "\\'" substring support.
* [#11121](http://example.invalid/ticket/11121): [Firefox] Fixed: High Contrast mode is enabled when the editor is loaded in a hidden iframe.
* [#11350](http://example.invalid/ticket/11350): The default value of [`config.contentsCss`](http://example.invalid/#!/api/CKEDITOR.config-cfg-contentsCss) is affected by [`CKEDITOR.getUrl()`](http://example.invalid/#!/api/CKEDITOR-method-getUrl).
* [#11097](http://example.invalid/ticket/11097): Improved the [Autogrow](http://example.invalid/addon/autogrow) plugin performance when dealing with very big tables.
* [#11290](http://example.invalid/ticket/11290): Removed redundant code in the [Source Dialog](http://example.invalid/addon/sourcedialog) plugin.
* [#11133](http://example.invalid/ticket/11133): [Page Break](http://example.invalid/addon/pagebreak) becomes editable if pasted.
* [#11126](http://example.invalid/ticket/11126): Fixed: Native Undo executed once the bottom of the snapshot stack is reached.
* [#11131](http://example.invalid/ticket/11131): [Div Editing Area](http://example.invalid/addon/divarea): Fixed: Error thrown when switching to source mode if the selection was in widget's nested editable.
* [#11139](http://example.invalid/ticket/11139): [Div Editing Area](http://example.invalid/addon/divarea): Fixed: Elements Path is not cleared after switching to source mode.
* [#10778](http://example.invalid/ticket/10778): Fixed a bug with range enlargement. The range no longer expands to visible whitespace.
* [#11146](http://example.invalid/ticket/11146): [IE] Fixed: Preview window switches Internet Explorer to Quirks Mode.
* [#10762](http://example.invalid/ticket/10762): [IE] Fixed: JavaScript code displayed in preview window's URL bar.
* [#11186](http://example.invalid/ticket/11186): Introduced the [`widgets.repository.addUpcastCallback()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.repository-method-addUpcastCallback) method that allows to block upcasting given element to a widget.
* [#11307](http://example.invalid/ticket/11307): Fixed: Paste as Plain Text conflict with the [MooTools](http://example.invalid) library.
* [#11140](http://example.invalid/ticket/11140): [IE11] Fixed: Anchors are not draggable.
* [#11379](http://example.invalid/ticket/11379): Changed default contents `line-height` to unitless values to avoid huge text overlapping (like in [#9696](http://example.invalid/ticket/9696)).
* [#10787](http://example.invalid/ticket/10787): [Firefox] Fixed: Broken replacement of text while pasting into `div`-based editor.
* [#10884](http://example.invalid/ticket/10884): Widgets integration with the [Show Blocks](http://example.invalid/addon/showblocks) plugin.
* [#11021](http://example.invalid/ticket/11021): Fixed: An error thrown when selecting entire editable contents while fake selection is on.
* [#11086](http://example.invalid/ticket/11086): [IE8] Re-enable inline widgets drag&drop in Internet Explorer 8.
* [#11372](http://example.invalid/ticket/11372): Widgets: Special characters encoded twice in nested editables.
* [#10068](http://example.invalid/ticket/10068): Fixed: Support for protocol-relative URLs.
* [#11283](http://example.invalid/ticket/11283): [Enhanced Image](http://example.invalid/addon/image2): A `<div>` element with `text-align: center` and an image inside is not recognised correctly.
* [#11196](http://example.invalid/ticket/11196): [Accessibility Instructions](http://example.invalid/addon/a11yhelp): Allowed additional keyboard button labels to be translated in the dialog window.

## CKEditor 4.3.1

**Important Notes:**

* To match the naming convention, the `language` button is now `Language` ([#11201](http://example.invalid/ticket/11201)).
* [Enhanced Image](http://example.invalid/addon/image2) button, context menu, command, and icon names match those of the [Image](http://example.invalid/addon/image) plugin ([#11222](http://example.invalid/ticket/11222)).

Fixed Issues:

* [#11244](http://example.invalid/ticket/11244): Changed: The [`widget.repository.checkWidgets()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.repository-method-checkWidgets) method now fires the [`widget.repository.checkWidgets`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.repository-event-checkWidgets) event, so from CKEditor 4.3.1 it is preferred to use the method rather than fire the event.
* [#11171](http://example.invalid/ticket/11171): Fixed: [`editor.insertElement()`](http://example.invalid/#!/api/CKEDITOR.editor-method-insertElement) and [`editor.insertText()`](http://example.invalid/#!/api/CKEDITOR.editor-method-insertText) methods do not call the [`widget.repository.checkWidgets()`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.repository-method-checkWidgets) method.
* [#11085](http://example.invalid/ticket/11085): [IE8] Replaced preview generated by the [Mathematical Formulas](http://example.invalid/addon/mathjax) widget with a placeholder.
* [#11044](http://example.invalid/ticket/11044): Enhanced WAI-ARIA support for the [Language](http://example.invalid/addon/language) plugin drop-down menu.
* [#11075](http://example.invalid/ticket/11075): With drop-down menu button focused, pressing the *Down Arrow* key will now open the menu and focus its first option.
* [#11165](http://example.invalid/ticket/11165): Fixed: The [File Browser](http://example.invalid/addon/filebrowser) plugin cannot be removed from the editor.
* [#11159](http://example.invalid/ticket/11159): [IE9-10] [Enhanced Image](http://example.invalid/addon/image2): Fixed buggy discovery of image dimensions.
* [#11101](http://example.invalid/ticket/11101): Drop-down lists no longer break when given double quotes.
* [#11077](http://example.invalid/ticket/11077): [Enhanced Image](http://example.invalid/addon/image2): Empty undo step recorded when resizing the image.
* [#10853](http://example.invalid/ticket/10853): [Enhanced Image](http://example.invalid/addon/image2): Widget has paragraph wrapper when de-captioning unaligned image.
* [#11198](http://example.invalid/ticket/11198): Widgets: Drag handler is not fully visible when an inline widget is in a heading.
* [#11132](http://example.invalid/ticket/11132): [Firefox] Fixed: Caret is lost after drag and drop of an inline widget.
* [#11182](http://example.invalid/ticket/11182): [IE10-11] Fixed: Editor crashes (IE11) or works with minor issues (IE10) if a page is loaded in Quirks Mode. See [`env.quirks`](http://example.invalid/#!/api/CKEDITOR.env-property-quirks) for more details.
* [#11204](http://example.invalid/ticket/11204): Added `figure` and `figcaption` styles to the `contents.css` file so [Enhanced Image](http://example.invalid/addon/image2) looks nicer.
* [#11202](http://example.invalid/ticket/11202): Fixed: No newline in [BBCode](http://example.invalid/addon/bbcode) mode.
* [#10890](http://example.invalid/ticket/10890): Fixed: Error thrown when pressing the *Delete* key in a list item.
* [#10055](http://example.invalid/ticket/10055): [IE8-10] Fixed: *Delete* pressed on a selected image causes the browser to go back.
* [#11183](http://example.invalid/ticket/11183): Fixed: Inserting a horizontal rule or a table in multiple row selection causes a browser crash. Additionally, the [`editor.insertElement()`](http://example.invalid/#!/api/CKEDITOR.editor-method-insertElement) method does not insert the element into every range of a selection any more.
* [#11042](http://example.invalid/ticket/11042): Fixed: Selection made on an element containing a non-editable element was not auto faked.
* [#11125](http://example.invalid/ticket/11125): Fixed: Keyboard navigation through menu and drop-down items will now cycle.
* [#11011](http://example.invalid/ticket/11011): Fixed: The [`editor.applyStyle()`](http://example.invalid/#!/api/CKEDITOR.editor-method-applyStyle) method removes attributes from nested elements.
* [#11179](http://example.invalid/ticket/11179): Fixed: [`editor.destroy()`](http://example.invalid/#!/api/CKEDITOR.editor-method-destroy) does not cleanup content generated by the [Table Resize](http://example.invalid/addon/tableresize) plugin for inline editors.
* [#11237](http://example.invalid/ticket/11237): Fixed: Table border attribute value is deleted when pasting content from Microsoft Word.
* [#11250](http://example.invalid/ticket/11250): Fixed: HTML entities inside the `<textarea>` element are not encoded.
* [#11260](http://example.invalid/ticket/11260): Fixed: Initially disabled buttons are not read by JAWS as disabled.
* [#11200](http://example.invalid/ticket/11200):  Added [Clipboard](http://example.invalid/addon/clipboard) plugin as a dependency for [Widget](http://example.invalid/addon/widget) to fix drag and drop.

## CKEditor 4.3

New Features:

* [#10612](http://example.invalid/ticket/10612): Internet Explorer 11 support.
* [#10869](http://example.invalid/ticket/10869): Widgets: Added better integration with the [Elements Path](http://example.invalid/addon/elementspath) plugin.
* [#10886](http://example.invalid/ticket/10886): Widgets: Added tooltip to the drag handle.
* [#10933](http://example.invalid/ticket/10933): Widgets: Introduced drag and drop of block widgets with the [Line Utilities](http://example.invalid/addon/lineutils) plugin.
* [#10936](http://example.invalid/ticket/10936): Widget System changes for easier integration with other dialog systems.
* [#10895](http://example.invalid/ticket/10895): [Enhanced Image](http://example.invalid/addon/image2): Added file browser integration.
* [#11002](http://example.invalid/ticket/11002): Added the [`draggable`](http://example.invalid/#!/api/CKEDITOR.plugins.widget.definition-property-draggable) option to disable drag and drop support for widgets.
* [#10937](http://example.invalid/ticket/10937): [Mathematical Formulas](http://example.invalid/addon/mathjax) widget improvements:
  * loading indicator ([#10948](http://example.invalid/ticket/10948)),
  * applying paragraph changes (like font color change) to iframe ([#10841](http://example.invalid/ticket/10841)),
  * Firefox and IE9 clipboard fixes ([#10857](http://example.invalid/ticket/10857)),
  * fixing same origin policy issue ([#10840](http://example.invalid/ticket/10840)),
  * fixing undo bugs ([#10842](http://example.invalid/ticket/10842), [#10930](http://example.invalid/ticket/10930)),
  * fixing other minor bugs.
* [#10862](http://example.invalid/ticket/10862): [Placeholder](http://example.invalid/addon/placeholder) plugin was rewritten as a widget.
* [#10822](http://example.invalid/ticket/10822): Added styles system integration with non-editable elements (for example widgets) and their nested editables. Styles cannot change non-editable content and are applied in nested editable only if allowed by its type and content filter.
* [#10856](http://example.invalid/ticket/10856): Menu buttons will now toggle the visibility of their panels when clicked multiple times. [Language](http://example.invalid/addon/language) plugin fixes: Added active language highlighting, added an option to remove the language.
* [#10028](http://example.invalid/ticket/10028): New [`config.dialog_noConfirmCancel`](http://example.invalid/#!/api/CKEDITOR.config-cfg-dialog_noConfirmCancel) configuration option that eliminates the need to confirm closing of a dialog window when the user changed any of its fields.
* [#10848](http://example.invalid/ticket/10848): Integrate remaining plugins ([Styles](http://example.invalid/addon/stylescombo), [Format](http://example.invalid/addon/format), [Font](http://example.invalid/addon/font), [Color Button](http://example.invalid/addon/colorbutton), [Language](http://example.invalid/addon/language) and [Indent](http://example.invalid/addon/indent)) with [active filter](http://example.invalid/#!/api/CKEDITOR.editor-property-activeFilter).
* [#10855](http://example.invalid/ticket/10855): Change the extension of emoticons in the [BBCode](http://example.invalid/addon/bbcode) sample from GIF to PNG.

Fixed Issues:

* [#10831](http://example.invalid/ticket/10831): [Enhanced Image](http://example.invalid/addon/image2): Merged `image2inline` and `image2block` into one `image2` widget.
* [#10835](http://example.invalid/ticket/10835): [Enhanced Image](http://example.invalid/addon/image2): Improved visibility of the resize handle.
* [#10836](http://example.invalid/ticket/10836): [Enhanced Image](http://example.invalid/addon/image2): Preserve custom mouse cursor while resizing the image.
* [#10939](http://example.invalid/ticket/10939): [Firefox] [Enhanced Image](http://example.invalid/addon/image2): hovering the image causes it to change.
* [#10866](http://example.invalid/ticket/10866): Fixed: Broken *Tab* key navigation in the [Enhanced Image](http://example.invalid/addon/image2) dialog window.
* [#10833](http://example.invalid/ticket/10833): Fixed: *Lock ratio* option should be on by default in the [Enhanced Image](http://example.invalid/addon/image2) dialog window.
* [#10881](http://example.invalid/ticket/10881): Various improvements to *Enter* key behavior in nested editables.
* [#10879](http://example.invalid/ticket/10879): [Remove Format](http://example.invalid/addon/removeformat) should not leak from a nested editable.
* [#10877](http://example.invalid/ticket/10877): Fixed: [WebSpellChecker](http://example.invalid/addon/wsc) fails to apply changes if a nested editable was focused.
* [#10877](http://example.invalid/ticket/10877): Fixed: [SCAYT](http://example.invalid/addon/wsc) blocks typing in nested editables.
* [#11079](http://example.invalid/ticket/11079): Add button icons to the [Placeholder](http://example.invalid/addon/placeholder) sample.
* [#10870](http://example.invalid/ticket/10870): The `paste` command is no longer being disabled when the clipboard is empty.
* [#10854](http://example.invalid/ticket/10854): Fixed: Firefox prepends `<br>` to `<body>`, so it is stripped by the HTML data processor.
* [#10823](http://example.invalid/ticket/10823): Fixed: [Link](http://example.invalid/addon/link) plugin does not work with non-editable content.
* [#10828](http://example.invalid/ticket/10828): [Magic Line](http://example.invalid/addon/magicline) integration with the Widget System.
* [#10865](http://example.invalid/ticket/10865): Improved hiding copybin, so copying widgets works smoothly.
* [#11066](http://example.invalid/ticket/11066): Widget's private parts use CSS reset.
* [#11027](http://example.invalid/ticket/11027): Fixed: Block commands break on widgets; added the [`contentDomInvalidated`](http://example.invalid/#!/api/CKEDITOR.editor-event-contentDomInvalidated) event.
* [#10430](http://example.invalid/ticket/10430): Resolve dependence of the [Image](http://example.invalid/addon/image) plugin on the [Form Elements](http://example.invalid/addon/forms) plugin.
* [#10911](http://example.invalid/ticket/10911): Fixed: Browser *Alt* hotkeys will no longer be blocked while a widget is focused.
* [#11082](http://example.invalid/ticket/11082): Fixed: Selected widget is not copied or cut when using toolbar buttons or context menu.
* [#11083](http://example.invalid/ticket/11083): Fixed list and div element application to block widgets.
* [#10887](http://example.invalid/ticket/10887): Internet Explorer 8 compatibility issues related to the Widget System.
* [#11074](http://example.invalid/ticket/11074): Temporarily disabled inline widget drag and drop, because of seriously buggy native `range#moveToPoint` method.
* [#11098](http://example.invalid/ticket/11098): Fixed: Wrong selection position after undoing widget drag and drop.
* [#11110](http://example.invalid/ticket/11110): Fixed: IFrame and Flash objects are being incorrectly pasted in certain conditions.
* [#11129](http://example.invalid/ticket/11129): Page break is lost when loading data.
* [#11123](http://example.invalid/ticket/11123): [Firefox] Widget is destroyed after being dragged outside of `<body>`.
* [#11124](http://example.invalid/ticket/11124): Fixed the [Elements Path](http://example.invalid/addon/elementspath) in an editor using the [Div Editing Area](http://example.invalid/addon/divarea).

## CKEditor 4.3 Beta

New Features:

* [#9764](http://example.invalid/ticket/9764): Widget System.
  * [Widget plugin](http://example.invalid/addon/widget) introducing the [Widget API](http://example.invalid/#!/api/CKEDITOR.plugins.widget).
  * New [`editor.enterMode`](http://example.invalid/#!/api/CKEDITOR.editor-property-enterMode) and [`editor.shiftEnterMode`](http://example.invalid/#!/api/CKEDITOR.editor-property-shiftEnterMode) properties &ndash; normalized versions of [`config.enterMode`](http://example.invalid/#!/api/CKEDITOR.config-cfg-enterMode) and [`config.shiftEnterMode`](http://example.invalid/#!/api/CKEDITOR.config-cfg-shiftEnterMode).
  * Dynamic editor settings. Starting from CKEditor 4.3 Beta, *Enter* mode values and [content filter](http://example.invalid/#!/guide/dev_advanced_content_filter) instances may be changed dynamically (for example when the caret was placed in an element in which editor features should be adjusted). When you are implementing a new editor feature, you should base its behavior on [dynamic](http://example.invalid/#!/api/CKEDITOR.editor-property-activeEnterMode) or [static](http://example.invalid/#!/api/CKEDITOR.editor-property-enterMode) *Enter* mode values depending on whether this feature works in selection context or globally on editor content.
      * Dynamic *Enter* mode values &ndash; [`editor.setActiveEnterMode()`](http://example.invalid/#!/api/CKEDITOR.editor-method-setActiveEnterMode) method, [`editor.activeEnterModeChange`](http://example.invalid/#!/api/CKEDITOR.editor-event-activeEnterModeChange) event, and two properties: [`editor.activeEnterMode`](http://example.invalid/#!/api/CKEDITOR.editor-property-activeEnterMode) and [`editor.activeShiftEnterMode`](http://example.invalid/#!/api/CKEDITOR.editor-property-activeShiftEnterMode).
      * Dynamic content filter instances &ndash; [`editor.setActiveFilter()`](http://example.invalid/#!/api/CKEDITOR.editor-method-setActiveFilter) method, [`editor.activeFilterChange`](http://example.invalid/#!/api/CKEDITOR.editor-event-activeFilterChange) event, and [`editor.activeFilter`](http://example.invalid/#!/api/CKEDITOR.editor-property-activeFilter) property.
  * "Fake" selection was introduced. It makes it possible to virtually select any element when the real selection remains hidden. See the  [`selection.fake()`](http://example.invalid/#!/api/CKEDITOR.dom.selection-method-fake) method.
  * Default [`htmlParser.filter`](http://example.invalid/#!/api/CKEDITOR.htmlParser.filter) rules are not applied to non-editable elements (elements with `contenteditable` attribute set to `false` and their descendants) anymore. To add a rule which will be applied to all elements you need to pass an additional argument to the [`filter.addRules()`](http://example.invalid/#!/api/CKEDITOR.htmlParser.filter-method-addRules) method.
  * Dozens of new methods were introduced &ndash; most interesting ones:
      * [`document.find()`](http://example.invalid/#!/api/CKEDITOR.dom.document-method-find),
      * [`document.findOne()`](http://example.invalid/#!/api/CKEDITOR.dom.document-method-findOne),
      * [`editable.insertElementIntoRange()`](http://example.invalid/#!/api/CKEDITOR.editable-method-insertElementIntoRange),
      * [`range.moveToClosestEditablePosition()`](http://example.invalid/#!/api/CKEDITOR.dom.range-method-moveToClosestEditablePosition),
      * New methods for [`htmlParser.node`](http://example.invalid/#!/api/CKEDITOR.htmlParser.node) and [`htmlParser.element`](http://example.invalid/#!/api/CKEDITOR.htmlParser.element).
* [#10659](http://example.invalid/ticket/10659): New [Enhanced Image](http://example.invalid/addon/image2) plugin that introduces a widget with integrated image captions, an option to center images, and dynamic "click and drag" resizing.
* [#10664](http://example.invalid/ticket/10664): New [Mathematical Formulas](http://example.invalid/addon/mathjax) plugin that introduces the MathJax widget.
* [#7987](https://example.invalid/ticket/7987): New [Language](http://example.invalid/addon/language) plugin that implements Language toolbar button to support [WCAG 3.1.2 Language of Parts](http://example.invalid/TR/UNDERSTANDING-WCAG20/meaning-other-lang-id.html).
* [#10708](http://example.invalid/ticket/10708): New [smileys](http://example.invalid/addon/smiley).

## CKEditor 4.2.3

Fixed Issues:

* [#10994](http://example.invalid/ticket/10994): Fixed: Loading external jQuery library when opening the [jQuery Adapter](http://example.invalid/#!/guide/dev_jquery) sample directly from file.
* [#10975](http://example.invalid/ticket/10975): [IE] Fixed: Error thrown while opening the color palette.
* [#9929](http://example.invalid/ticket/9929): [Blink/WebKit] Fixed: A non-breaking space is created once a character is deleted and a regular space is typed.
* [#10963](http://example.invalid/ticket/10963): Fixed: JAWS issue with the keyboard shortcut for [Magic Line](http://example.invalid/addon/magicline).
* [#11096](http://example.invalid/ticket/11096): Fixed: TypeError: Object has no method 'is'.

## CKEditor 4.2.2

Fixed Issues:

* [#9314](http://example.invalid/ticket/9314): Fixed: Incorrect error message on closing a dialog window without saving changs.
* [#10308](http://example.invalid/ticket/10308): [IE10] Fixed: Unspecified error when deleting a row.
* [#10945](http://example.invalid/ticket/10945): [Chrome] Fixed: Clicking with a mouse inside the editor does not show the caret.
* [#10912](http://example.invalid/ticket/10912): Prevent default action when content of a non-editable link is clicked.
* [#10913](http://example.invalid/ticket/10913): Fixed [`CKEDITOR.plugins.addExternal()`](http://example.invalid/#!/api/CKEDITOR.resourceManager-method-addExternal) not handling paths including file name specified.
* [#10666](http://example.invalid/ticket/10666): Fixed [`CKEDITOR.tools.isArray()`](http://example.invalid/#!/api/CKEDITOR.tools-method-isArray) not working cross frame.
* [#10910](http://example.invalid/ticket/10910): [IE9] Fixed JavaScript error thrown in Compatibility Mode when clicking and/or typing in the editing area.
* [#10868](http://example.invalid/ticket/10868): [IE8] Prevent the browser from crashing when applying the Inline Quotation style.
* [#10915](http://example.invalid/ticket/10915): Fixed: Invalid CSS filter in the Kama skin.
* [#10914](http://example.invalid/ticket/10914): Plugins [Indent List](http://example.invalid/addon/indentlist) and [Indent Block](http://example.invalid/addon/indentblock) are now included in the build configuration.
* [#10812](http://example.invalid/ticket/10812): Fixed [`range.createBookmark2()`](http://example.invalid/#!/api/CKEDITOR.dom.range-method-createBookmark2) incorrectly normalizing offsets. This bug was causing many issues: [#10850](http://example.invalid/ticket/10850), [#10842](http://example.invalid/ticket/10842).
* [#10951](http://example.invalid/ticket/10951): Reviewed and optimized focus handling on panels (combo, menu buttons, color buttons, and context menu) to enhance accessibility. Fixed [#10705](http://example.invalid/ticket/10705), [#10706](http://example.invalid/ticket/10706) and [#10707](http://example.invalid/ticket/10707).
* [#10704](http://example.invalid/ticket/10704): Fixed a JAWS issue with the Select Color dialog window title not being announced.
* [#10753](http://example.invalid/ticket/10753): The floating toolbar in inline instances now has a dedicated accessibility label.

## CKEditor 4.2.1

Fixed Issues:

* [#10301](http://example.invalid/ticket/10301): [IE9-10] Undo fails after 3+ consecutive paste actions with a JavaScript error.
* [#10689](http://example.invalid/ticket/10689): Save toolbar button saves only the first editor instance.
* [#10368](http://example.invalid/ticket/10368): Move language reading direction definition (`dir`) from main language file to core.
* [#9330](http://example.invalid/ticket/9330): Fixed pasting anchors from MS Word.
* [#8103](http://example.invalid/ticket/8103): Fixed pasting nested lists from MS Word.
* [#9958](http://example.invalid/ticket/9958): [IE9] Pressing the "OK" button will trigger the `onbeforeunload` event in the popup dialog.
* [#10662](http://example.invalid/ticket/10662): Fixed styles from the Styles drop-down list not registering to the ACF in case when the [Shared Spaces plugin](http://example.invalid/addon/sharedspace) is used.
* [#9654](http://example.invalid/ticket/9654): Problems with Internet Explorer 10 Quirks Mode.
* [#9816](http://example.invalid/ticket/9816): Floating toolbar does not reposition vertically in several cases.
* [#10646](http://example.invalid/ticket/10646): Removing a selected sublist or nested table with *Backspace/Delete* removes the parent element.
* [#10623](http://example.invalid/ticket/10623): [WebKit] Page is scrolled when opening a drop-down list.
* [#10004](http://example.invalid/ticket/10004): [ChromeVox] Button names are not announced.
* [#10731](http://example.invalid/ticket/10731): [WebSpellChecker](http://example.invalid/addon/wsc) plugin breaks cloning of editor configuration.
* It is now possible to set per instance [WebSpellChecker](http://example.invalid/addon/wsc) plugin configuration instead of setting the configuration globally.

## CKEditor 4.2

**Important Notes:**

* Dropped compatibility support for Internet Explorer 7 and Firefox 3.6.

* Both the Basic and the Standard distribution packages will not contain the new [Indent Block](http://example.invalid/addon/indentblock) plugin. Because of this the [Advanced Content Filter](http://example.invalid/#!/guide/dev_advanced_content_filter) might remove block indentations from existing contents. If you want to prevent this, either [add an appropriate ACF rule to your filter](http://example.invalid/#!/guide/dev_allowed_content_rules) or create a custom build based on the Basic/Standard package and add the Indent Block plugin in [CKBuilder](http://example.invalid/builder).

New Features:

* [#10027](http://example.invalid/ticket/10027): Separated list and block indentation into two plugins: [Indent List](http://example.invalid/addon/indentlist) and [Indent Block](http://example.invalid/addon/indentblock).
* [#8244](http://example.invalid/ticket/8244): Use *(Shift+)Tab* to indent and outdent lists.
* [#10281](http://example.invalid/ticket/10281): The [jQuery Adapter](http://example.invalid/#!/guide/dev_jquery) is now available. Several jQuery-related issues fixed: [#8261](http://example.invalid/ticket/8261), [#9077](http://example.invalid/ticket/9077), [#8710](http://example.invalid/ticket/8710), [#8530](http://example.invalid/ticket/8530), [#9019](http://example.invalid/ticket/9019), [#6181](http://example.invalid/ticket/6181), [#7876](http://example.invalid/ticket/7876), [#6906](http://example.invalid/ticket/6906).
* [#10042](http://example.invalid/ticket/10042): Introduced [`config.title`](http://example.invalid/#!/api/CKEDITOR.config-cfg-title) setting to change the human-readable title of the editor.
* [#9794](http://example.invalid/ticket/9794): Added [`editor.change`](http://example.invalid/#!/api/CKEDITOR.editor-event-change) event.
* [#9923](http://example.invalid/ticket/9923): HiDPI support in the editor UI. HiDPI icons for [Moono skin](http://example.invalid/addon/moono) added.
* [#8031](http://example.invalid/ticket/8031): Handle `required` attributes on `<textarea>` elements &mdash; introduced [`editor.required`](http://example.invalid/#!/api/CKEDITOR.editor-event-required) event.
* [#10280](http://example.invalid/ticket/10280): Ability to replace `<textarea>` elements with the inline editor.

Fixed Issues:

* [#10599](http://example.invalid/ticket/10599): [Indent](http://example.invalid/addon/indent) plugin is no longer required by the [List](http://example.invalid/addon/list) plugin.
* [#10370](http://example.invalid/ticket/10370): Inconsistency in data events between framed and inline editors.
* [#10438](http://example.invalid/ticket/10438): [FF, IE] No selection is done on an editable element on executing [`editor.setData()`](http://example.invalid/#!/api/CKEDITOR.editor-method-setData).

## CKEditor 4.1.3

New Features:

* Added new translation: Indonesian.

Fixed Issues:

* [#10644](http://example.invalid/ticket/10644): Fixed a critical bug when pasting plain text in Blink-based browsers.
* [#5189](http://example.invalid/ticket/5189): [Find/Replace](http://example.invalid/addon/find) dialog window: rename "Cancel" button to "Close".
* [#10562](http://example.invalid/ticket/10562): [Housekeeping] Unified CSS gradient filter formats in the [Moono](http://example.invalid/addon/moono) skin.
* [#10537](http://example.invalid/ticket/10537): Advanced Content Filter should register a default rule for [`config.shiftEnterMode`](http://example.invalid/#!/api/CKEDITOR.config-cfg-shiftEnterMode).
* [#10610](http://example.invalid/ticket/10610): [`CKEDITOR.dialog.addIframe()`](http://example.invalid/#!/api/CKEDITOR.dialog-static-method-addIframe) incorrectly sets the iframe size in dialog windows.

## CKEditor 4.1.2

New Features:

* Added new translation: Sinhala.

Fixed Issues:

* [#10339](http://example.invalid/ticket/10339): Fixed: Error thrown when inserted data was totally stripped out after filtering and processing.
* [#10298](http://example.invalid/ticket/10298): Fixed: Data processor breaks attributes containing protected parts.
* [#10367](http://example.invalid/ticket/10367): Fixed: [`editable.insertText()`](http://example.invalid/#!/api/CKEDITOR.editable-method-insertText) loses characters when `RegExp` replace controls are being inserted.
* [#10165](http://example.invalid/ticket/10165): [IE] Access denied error when `document.domain` has been altered.
* [#9761](http://example.invalid/ticket/9761): Update the *Backspace* key state in [`keystrokeHandler.blockedKeystrokes`](http://example.invalid/#!/api/CKEDITOR.keystrokeHandler-property-blockedKeystrokes) when calling [`editor.setReadOnly()`](http://example.invalid/#!/api/CKEDITOR.editor-method-setReadOnly).
* [#6504](http://example.invalid/ticket/6504): Fixed: Race condition while loading several [`config.customConfig`](http://example.invalid/#!/api/CKEDITOR.config-cfg-customConfig) files.
* [#10146](http://example.invalid/ticket/10146): [Firefox] Empty lines are being removed while [`config.enterMode`](http://example.invalid/#!/api/CKEDITOR.config-cfg-enterMode) is [`CKEDITOR.ENTER_BR`](http://example.invalid/#!/api/CKEDITOR-property-ENTER_BR).
* [#10360](http://example.invalid/ticket/10360): Fixed: ARIA `role="application"` should not be used for dialog windows.
* [#10361](http://example.invalid/ticket/10361): Fixed: ARIA `role="application"` should not be used for floating panels.
* [#10510](http://example.invalid/ticket/10510): Introduced unique voice labels to differentiate between different editor instances.
* [#9945](http://example.invalid/ticket/9945): [iOS] Scrolling not possible on iPad.
* [#10389](http://example.invalid/ticket/10389): Fixed: Invalid HTML in the "Text and Table" template.
* [WebSpellChecker](http://example.invalid/addon/wsc) plugin user interface was changed to match CKEditor 4 style.

## CKEditor 4.1.1

New Features:

* Added new translation: Albanian.

Fixed Issues:

* [#10172](http://example.invalid/ticket/10172): Pressing *Delete* or *Backspace* in an empty table cell moves the cursor to the next/previous cell.
* [#10219](http://example.invalid/ticket/10219): Error thrown when destroying an editor instance in parallel with a `mouseup` event.
* [#10265](http://example.invalid/ticket/10265): Wrong loop type in the [File Browser](http://example.invalid/addon/filebrowser) plugin.
* [#10249](http://example.invalid/ticket/10249): Wrong undo/redo states at start.
* [#10268](http://example.invalid/ticket/10268): [Show Blocks](http://example.invalid/addon/showblocks) does not recover after switching to Source view.
* [#9995](http://example.invalid/ticket/9995): HTML code in the `<textarea>` should not be modified by the [`htmlDataProcessor`](http://example.invalid/#!/api/CKEDITOR.htmlDataProcessor).
* [#10320](http://example.invalid/ticket/10320): [Justify](http://example.invalid/addon/justify) plugin should add elements to Advanced Content Filter based on current [Enter mode](http://example.invalid/#!/api/CKEDITOR.config-cfg-enterMode).
* [#10260](http://example.invalid/ticket/10260): Fixed: Advanced Content Filter blocks [`tabSpaces`](http://example.invalid/#!/api/CKEDITOR.config-cfg-tabSpaces). Unified `data-cke-*` attributes filtering.
* [#10315](http://example.invalid/ticket/10315): [WebKit] [Undo manager](http://example.invalid/#!/api/CKEDITOR.plugins.undo.UndoManager) should not record snapshots after a filling character was added/removed.
* [#10291](http://example.invalid/ticket/10291): [WebKit] Space after a filling character should be secured.
* [#10330](http://example.invalid/ticket/10330): [WebKit] The filling character is not removed on `keydown` in specific cases.
* [#10285](http://example.invalid/ticket/10285): Fixed: Styled text pasted from MS Word causes an infinite loop.
* [#10131](http://example.invalid/ticket/10131): Fixed: [`undoManager.update()`](http://example.invalid/#!/api/CKEDITOR.plugins.undo.UndoManager-method-update) does not refresh the command state.
* [#10337](http://example.invalid/ticket/10337): Fixed: Unable to remove `<s>` using [Remove Format](http://example.invalid/addon/removeformat).

## CKEditor 4.1

Fixed Issues:

* [#10192](http://example.invalid/ticket/10192): Closing lists with the *Enter* key does not work with [Advanced Content Filter](http://example.invalid/#!/guide/dev_advanced_content_filter) in several cases.
* [#10191](http://example.invalid/ticket/10191): Fixed allowed content rules unification, so the [`filter.allowedContent`](http://example.invalid/#!/api/CKEDITOR.filter-property-allowedContent) property always contains rules in the same format.
* [#10224](http://example.invalid/ticket/10224): Advanced Content Filter does not remove non-empty `<a>` elements anymore.
* Minor issues in plugin integration with Advanced Content Filter:
  * [#10166](http://example.invalid/ticket/10166): Added transformation from the `align` attribute to `float` style to preserve backward compatibility after the introduction of Advanced Content Filter.
  * [#10195](http://example.invalid/ticket/10195): [Image](http://example.invalid/addon/image) plugin no longer registers rules for links to Advanced Content Filter.
  * [#10213](http://example.invalid/ticket/10213): [Justify](http://example.invalid/addon/justify) plugin is now correctly registering rules to Advanced Content Filter when [`config.justifyClasses`](http://example.invalid/#!/api/CKEDITOR.config-cfg-justifyClasses) is defined.

## CKEditor 4.1 RC

New Features:

* [#9829](http://example.invalid/ticket/9829): Advanced Content Filter - data and features activation based on editor configuration.

  Brand new data filtering system that works in 2 modes:

  * Based on loaded features (toolbar items, plugins) - the data will be filtered according to what the editor in its
  current configuration can handle.
  * Based on [`config.allowedContent`](http://example.invalid/#!/api/CKEDITOR.config-cfg-allowedContent) rules - the data
  will be filtered and the editor features (toolbar items, commands, keystrokes) will be enabled if they are allowed.

  See the `datafiltering.html` sample, [guides](http://example.invalid/#!/guide/dev_advanced_content_filter) and [`CKEDITOR.filter` API documentation](http://example.invalid/#!/api/CKEDITOR.filter).
* [#9387](http://example.invalid/ticket/9387): Reintroduced [Shared Spaces](http://example.invalid/addon/sharedspace) - the ability to display toolbar and bottom editor space in selected locations and to share them by different editor instances.
* [#9907](http://example.invalid/ticket/9907): Added the [`contentPreview`](http://example.invalid/#!/api/CKEDITOR-event-contentPreview) event for preview data manipulation.
* [#9713](http://example.invalid/ticket/9713): Introduced the [Source Dialog](http://example.invalid/addon/sourcedialog) plugin that brings raw HTML editing for inline editor instances.
* Included in [#9829](http://example.invalid/ticket/9829): Introduced new events, [`toHtml`](http://example.invalid/#!/api/CKEDITOR.editor-event-toHtml) and [`toDataFormat`](http://example.invalid/#!/api/CKEDITOR.editor-event-toDataFormat), allowing for better integration with data processing.
* [#9981](http://example.invalid/ticket/9981): Added ability to filter [`htmlParser.fragment`](http://example.invalid/#!/api/CKEDITOR.htmlParser.fragment), [`htmlParser.element`](http://example.invalid/#!/api/CKEDITOR.htmlParser.element) etc. by many [`htmlParser.filter`](http://example.invalid/#!/api/CKEDITOR.htmlParser.filter)s before writing structure to an HTML string.
* Included in [#10103](http://example.invalid/ticket/10103):
  * Introduced the [`editor.status`](http://example.invalid/#!/api/CKEDITOR.editor-property-status) property to make it easier to check the current status of the editor.
  * Default [`command`](http://example.invalid/#!/api/CKEDITOR.command) state is now [`CKEDITOR.TRISTATE_DISABLE`](http://example.invalid/#!/api/CKEDITOR-property-TRISTATE_DISABLED). It will be activated on [`editor.instanceReady`](http://example.invalid/#!/api/CKEDITOR-event-instanceReady) or immediately after being added if the editor is already initialized.
* [#9796](http://example.invalid/ticket/9796): Introduced `<s>` as a default tag for strikethrough, which replaces obsolete `<strike>` in HTML5.

## CKEditor 4.0.3

Fixed Issues:

* [#10196](http://example.invalid/ticket/10196): Fixed context menus not opening with keyboard shortcuts when [Autogrow](http://example.invalid/addon/autogrow) is enabled.
* [#10212](http://example.invalid/ticket/10212): [IE7-10] Undo command throws errors after multiple switches between Source and WYSIWYG view.
* [#10219](http://example.invalid/ticket/10219): [Inline editor] Error thrown after calling [`editor.destroy()`](http://example.invalid/#!/api/CKEDITOR.editor-method-destroy).

## CKEditor 4.0.2

Fixed Issues:

* [#9779](http://example.invalid/ticket/9779): Fixed overriding [`CKEDITOR.getUrl()`](http://example.invalid/#!/api/CKEDITOR-method-getUrl) with `CKEDITOR_GETURL`.
* [#9772](http://example.invalid/ticket/9772): Custom buttons in the dialog window footer have different look and size ([Moono](http://example.invalid/addon/moono), [Kama](http://example.invalid/addon/kama) skins).
* [#9029](http://example.invalid/ticket/9029): Custom styles added with the [`stylesSet.add()`](http://example.invalid/#!/api/CKEDITOR.stylesSet-method-add) are displayed in the wrong order.
* [#9887](http://example.invalid/ticket/9887): Disable [Magic Line](http://example.invalid/addon/magicline) when [`editor.readOnly`](http://example.invalid/#!/api/CKEDITOR.editor-property-readOnly) is set.
* [#9882](http://example.invalid/ticket/9882): Fixed empty document title on [`editor.getData()`](http://example.invalid/#!/api/CKEDITOR.editor-method-getData) if set via the Document Properties dialog window.
* [#9773](http://example.invalid/ticket/9773): Fixed rendering problems with selection fields in the Kama skin.
* [#9851](http://example.invalid/ticket/9851): The [`selectionChange`](http://example.invalid/#!/api/CKEDITOR.editor-event-selectionChange) event is not fired when mouse selection ended outside editable.
* [#9903](http://example.invalid/ticket/9903): [Inline editor] Bad positioning of floating space with page horizontal scroll.
* [#9872](http://example.invalid/ticket/9872): [`editor.checkDirty()`](http://example.invalid/#!/api/CKEDITOR.editor-method-checkDirty) returns `true` when called onload. Removed the obsolete `editor.mayBeDirty` flag.
* [#9893](http://example.invalid/ticket/9893): [IE] Fixed broken toolbar when editing mixed direction content in Quirks mode.
* [#9845](http://example.invalid/ticket/9845): Fixed TAB navigation in the [Link](http://example.invalid/addon/link) dialog window when the Anchor option is used and no anchors are available.
* [#9883](http://example.invalid/ticket/9883): Maximizing was making the entire page editable with [divarea](http://example.invalid/addon/divarea)-based editors.
* [#9940](http://example.invalid/ticket/9940): [Firefox] Navigating back to a page with the editor was making the entire page editable.
* [#9966](http://example.invalid/ticket/9966): Fixed: Unable to type square brackets with French keyboard layout. Changed [Magic Line](http://example.invalid/addon/magicline) keystrokes.
* [#9507](http://example.invalid/ticket/9507): [Firefox] Selection is moved before editable position when the editor is focused for the first time.
* [#9947](http://example.invalid/ticket/9947): [WebKit] Editor overflows parent container in some edge cases.
* [#10105](http://example.invalid/ticket/10105): Fixed: Broken [sourcearea](http://example.invalid/addon/sourcearea) view when an RTL language is set.
* [#10123](http://example.invalid/ticket/10123): [WebKit] Fixed: Several dialog windows have broken layout since the latest WebKit release.
* [#10152](http://example.invalid/ticket/10152): Fixed: Invalid ARIA property used on menu items.

## CKEditor 0.0.0.0

Fixed Issues:

* Security update: Added protection against XSS attack and possible path disclosure in the PHP sample.

## CKEditor 4.0.1

Fixed Issues:

* [#9655](http://example.invalid/ticket/9655): Support for IE Quirks Mode in the new [Moono skin](http://example.invalid/addon/moono).
* Accessibility issues (mainly in inline editor): [#9364](http://example.invalid/ticket/9364), [#9368](http://example.invalid/ticket/9368), [#9369](http://example.invalid/ticket/9369), [#9370](http://example.invalid/ticket/9370), [#9541](http://example.invalid/ticket/9541), [#9543](http://example.invalid/ticket/9543), [#9841](http://example.invalid/ticket/9841), [#9844](http://example.invalid/ticket/9844).
* [Magic Line](http://example.invalid/addon/magicline) plugin:
    * [#9481](http://example.invalid/ticket/9481): Added accessibility support for Magic Line.
    * [#9509](http://example.invalid/ticket/9509): Added Magic Line support for forms.
    * [#9573](http://example.invalid/ticket/9573): Magic Line does not disappear on `mouseout` in a specific case.
* [#9754](http://example.invalid/ticket/9754): [WebKit] Cutting & pasting simple unformatted text generates an inline wrapper in WebKit browsers.
* [#9456](http://example.invalid/ticket/9456): [Chrome] Properly paste bullet list style from MS Word.
* [#9699](http://example.invalid/ticket/9699), [#9758](http://example.invalid/ticket/9758): Improved selection locking when selecting by dragging.
* Context menu:
    * [#9712](http://example.invalid/ticket/9712): Opening the context menu destroys editor focus.
    * [#9366](http://example.invalid/ticket/9366): Context menu should be displayed over the floating toolbar.
    * [#9706](http://example.invalid/ticket/9706): Context menu generates a JavaScript error in inline mode when the editor is attached to a header element.
* [#9800](http://example.invalid/ticket/9800): Hide float panel when resizing the window.
* [#9721](http://example.invalid/ticket/9721): Padding in content of div-based editor puts the editing area under the bottom UI space.
* [#9528](http://example.invalid/ticket/9528): Host page `box-sizing` style should not influence the editor UI elements.
* [#9503](http://example.invalid/ticket/9503): [Form Elements](http://example.invalid/addon/forms) plugin adds context menu listeners only on supported input types. Added support for `tel`, `email`, `search` and `url` input types.
* [#9769](http://example.invalid/ticket/9769): Improved floating toolbar positioning in a narrow window.
* [#9875](http://example.invalid/ticket/9875): Table dialog window does not populate width correctly.
* [#8675](http://example.invalid/ticket/8675): Deleting cells in a nested table removes the outer table cell.
* [#9815](http://example.invalid/ticket/9815): Cannot edit dialog window fields in an editor initialized in the jQuery UI modal dialog.
* [#8888](http://example.invalid/ticket/8888): CKEditor dialog windows do not show completely in a small window.
* [#9360](http://example.invalid/ticket/9360): [Inline editor] Blocks shown for a `<div>` element stay permanently even after the user exits editing the `<div>`.
* [#9531](http://example.invalid/ticket/9531): [Firefox & Inline editor] Toolbar is lost when closing the Format drop-down list by clicking its button.
* [#9553](http://example.invalid/ticket/9553): Table width incorrectly set when the `border-width` style is specified.
* [#9594](http://example.invalid/ticket/9594): Cannot tab past CKEditor when it is in read-only mode.
* [#9658](http://example.invalid/ticket/9658): [IE9] Justify not working on selected images.
* [#9686](http://example.invalid/ticket/9686): Added missing contents styles for `<pre>` elements.
* [#9709](http://example.invalid/ticket/9709): [Paste from Word](http://example.invalid/addon/pastefromword) should not depend on configuration from other styles.
* [#9726](http://example.invalid/ticket/9726): Removed [Color Dialog](http://example.invalid/addon/colordialog) plugin dependency from [Table Tools](http://example.invalid/addon/tabletools).
* [#9765](http://example.invalid/ticket/9765): Toolbar Collapse command documented incorrectly in the [Accessibility Instructions](http://example.invalid/addon/a11yhelp) dialog window.
* [#9771](http://example.invalid/ticket/9771): [WebKit & Opera] Fixed scrolling issues when pasting.
* [#9787](http://example.invalid/ticket/9787): [IE9] `onChange` is not fired for checkboxes in dialogs.
* [#9842](http://example.invalid/ticket/9842): [Firefox 17] When opening a toolbar menu for the first time and pressing the *Down Arrow* key, focus goes to the next toolbar button instead of the menu options.
* [#9847](http://example.invalid/ticket/9847): [Elements Path](http://example.invalid/addon/elementspath) should not be initialized in the inline editor.
* [#9853](http://example.invalid/ticket/9853): [`editor.addRemoveFormatFilter()`](http://example.invalid/#!/api/CKEDITOR.editor-method-addRemoveFormatFilter) is exposed before it really works.
* [#8893](http://example.invalid/ticket/8893): Value of the [`pasteFromWordCleanupFile`](http://example.invalid/#!/api/CKEDITOR.config-cfg-pasteFromWordCleanupFile) configuration option is now taken from the instance configuration.
* [#9693](http://example.invalid/ticket/9693): Removed "Live Preview" checkbox from UI color picker.


## CKEditor 4.0

The first stable release of the new CKEditor 4 code line.

The CKEditor JavaScript API has been kept compatible with CKEditor 4, whenever
possible. The list of relevant changes can be found in the [API Changes page of
the CKEditor 4 documentation][1].

[1]: http://example.invalid/#!/guide/dev_api_changes "API Changes"
