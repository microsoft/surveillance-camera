/**
 * Copyright (c) 2012 Nokia Corporation.
 */

import QtQuick 1.1

// Qt Quick Components
import com.nokia.symbian 1.1

// for NotifHelper and NotificationModel
import custom 1.0


// Log view page that is application main page
// Shows alarm log history
Page {
    id: root

    // Application id for Notificatins API
    property string applicationId: "com.demo.notification.dn"

    // Is notifications enabled by user
    property bool notificationEnabled: false

    // Is application busy
    property bool applicationBusy: false

    // Which button is enabled
    function updateButtonVisibility()
    {
        if (notificationEnabled) {
            buttonRow.checkedButton = notificationOn;
        }
        else {
            buttonRow.checkedButton = notificationOff;
        }
    }

    // Enable or disable listening of Notifications API
    function enableNotifications(enable)
    {
        if (enable == notificationEnabled)
            return;

        if (enable) {
            // Enable Notificatins API
            notificationEnabled = true;
            notifHelper.registerApplication(applicationId);
            notifHelper.saveSetting("enabled",notificationEnabled);
        } else {
            // Disable Notificatins API
            notificationEnabled = false;
            notifHelper.unregisterApplication();
            notifHelper.saveSetting("enabled",notificationEnabled);
        }

        updateButtonVisibility();
    }

    // Notification helper QML item for using Notifications API
    NotifHelper {
        id: notifHelper

        // Alarm log history model
        notificationModel: notificationModel;

        // Alarm notification received
        onAlarmReceived: {
            // Show alarm
            alarmLoader.source = "Alarm.qml";
        }

        // Application is busy handling
        onBusy: {
            if (applicationBusy === isBusy)
                return;

            applicationBusy = isBusy;
            busyIndicator.running = applicationBusy;

            if (applicationBusy) {
                busyIndicator.opacity = 1;
            }
            else {
                busyIndicator.opacity = 0;
            }

            if (applicationBusy) {
                connectingDialog.open();
            }
            else {
                connectingDialog.close();
            }
        }

        // Notifications API error
        onNotificationError: {
            connectingDialog.close();
            errorDialog.open();
        }
    }

    // Alarm log history model
    NotificationModel {
        id: notificationModel
    }

    // Make needed initializations on page construction
    Component.onCompleted: {
        // Activate and register application to receive notifications
        notifHelper.activate();
        notifHelper.registerApplication(applicationId);

        // Is notification enabled or disabled by user?
        notificationEnabled = notifHelper.loadSetting("enabled",notificationEnabled);
        console.log("notificationEnabled = "+notificationEnabled);
    }

    // Listening Page status
    onStatusChanged: {
        // Set right button activated
        if (status === PageStatus.Active) {
            updateButtonVisibility();
        }
    }

    // Background image
    Image {
        source: "background.jpg"
        anchors.fill: parent
        fillMode: Image.PreserveAspectCrop
        opacity: 0.3
    }

    // Menu
    Menu {
        id: menu

        content: MenuLayout {
            MenuItem {
                text: "Clear log"
                onClicked: notificationModel.clear();
            }
            MenuItem {
                text: "Info"
                onClicked: {
                    root.pageStack.push(Qt.resolvedUrl("AboutView.qml"));
                }
            }
        }
    }

    // List item for the ListView
    Component {
        id: listItemDelegate

        ListItem {
            id: listItem

            Column {
                anchors {
                    top: parent.top; topMargin: platformStyle.paddingMedium
                    left: parent.left; leftMargin: platformStyle.paddingMedium
                }

                ListItemText {
                    id: titleText
                    mode: listItem.mode
                    role: "Title"
                    text: "Received "+receivedTimestamp
                }
                ListItemText {
                    id: subtitleText
                    mode: listItem.mode
                    role: "SubTitle"
                    text: "Sent "+sendTimestamp
                }
            }
        }
    }

    // List header for the ListView
    Component {
        id: listHeader

        ListHeading {
            id: listHeading

            ListItemText {
                id: headingText
                anchors.fill: listHeading.paddingItem
                role: "Heading"
                text: "Notification log"
            }
        }
    }

    // History list
    ListView {
        id: listView
        anchors.fill: parent
        header: listHeader
        model: notificationModel
        delegate: listItemDelegate
        clip: true
    }

    // Busy
    BusyIndicator {
        id: busyIndicator
        running: false
        opacity: 0
        width: 30
        height: 30
        anchors.left: listView.left
        anchors.top: listView.top
    }

    // Loader that loads Alarm
    Loader {
        id: alarmLoader
        anchors.fill: parent

        Connections {
            target: alarmLoader.item

            onClean: {
                alarmLoader.source = "";
            }
        }
    }

    // Dialog for showing ongoing communication with Notifications Service
    Dialog {
        id: connectingDialog
        property string dialogTitle : "Communicating"
        property string dialogMessage : "with Notifications Service"
        visualParent: parent

        title: [
            Label {
                text: connectingDialog.dialogTitle
                anchors.centerIn: parent
            }
        ]
        content: [
            Label {
                text: connectingDialog.dialogMessage
                anchors.centerIn: parent
            }
        ]
        buttons: [
            ToolButton {
                flat: false
                text: "Cancel"
                anchors.centerIn: parent
                onClicked: {connectingDialog.reject();}
            }
        ]
        onRejected: {
            // User cancels registration to Notifications API
            notifHelper.cancel();
            notificationEnabled = false;
            notifHelper.saveSetting("enabled",notificationEnabled);
            updateButtonVisibility();
        }
    }

    // Dialog for showing erro when activating Notifications API
    Dialog {
        id: errorDialog
        property string dialogTitle : "Error"
        property string dialogMessage : "Activating Notifications API error"
        visualParent: parent

        title: [
            Label {
                text: errorDialog.dialogTitle
                anchors.centerIn: parent
            }
        ]
        content: [
            Label {
                text: errorDialog.dialogMessage
                anchors.centerIn: parent
            }
        ]
        buttons: [
            ToolButton {
                flat: false
                text: "Close"
                anchors.centerIn: parent
                onClicked: {
                    errorDialog.accept();
                }
            }
        ]
        onAccepted: {
            notificationEnabled = false;
            notifHelper.saveSetting("enabled",notificationEnabled);
            updateButtonVisibility();
        }
    }

    // Buttons for the toolbar
    tools: ToolBarLayout {
        id: toolBarLayout

        ToolButton {
            flat: true
            iconSource: "toolbar-back"
            onClicked: {
                Qt.quit()
            }
        }

        ButtonRow {
            id: buttonRow

            ToolButton {
                id: notificationOn
                enabled: !applicationBusy
                text: "Alarm"
                onClicked: {
                    enableNotifications(true);
                }
            }
            ToolButton {
                id: notificationOff
                enabled: !applicationBusy
                text: "Alarm off"
                onClicked: {
                    enableNotifications(false);
                }
            }
        }

        ToolButton {
            iconSource: "toolbar-menu"
            onClicked: menu.open()
        }
    }
}
