/**
 * Copyright (c) 2012-2014 Microsoft Mobile.
 */

import QtQuick 1.1

// Qt Quick Components
import com.nokia.symbian 1.1


// Application main window
Window {
    id: window

    // Statusbar
    StatusBar {
        id: statusBar
        anchors.top: parent.top
    }

    // Page stack where all pages exists
    PageStack {
        id: pageStack

        anchors {
            top: statusBar.bottom
            left: parent.left
            right: parent.right
            bottom: toolBar.top
        }

        toolBar: toolBar

        Component.onCompleted: {
            // Activate first page
            pageStack.push(Qt.resolvedUrl("LogView.qml"));
        }
    }

    // Toolbar
    ToolBar {
        id: toolBar
        anchors.bottom: parent.bottom
    }
}
