/**
 * Copyright (c) 2012-2014 Microsoft Mobile.
 */

#include "notifhelper.h"

#include <QDate>
#include <QDebug>
#include <QPluginLoader>
#include <QSettings>
#include <QTime>
#include <QTimer>

#include "notificationmodel.h"

// Notifications API headers
#include <ovinotificationinterface.h>
#include <ovinotificationsession.h>
#include <ovinotificationinfo.h>
#include <ovinotificationmessage.h>
#include <ovinotificationstate.h>
#include <ovinotificationpayload.h>


/*!
  \class NotifHelper
  \brief Helper class for usage of Nokia Notifications API
*/


/*!
  Constructor
*/
NotifHelper::NotifHelper(QObject *parent) :
    QObject(parent),
    m_notificationSession(0),
    m_notificationModel(0),
    m_settings(0)
{
    // Application settings
    m_settings = new QSettings("Nokia", "AlarmListener", this);
}


/*!
  Destructor
*/
NotifHelper::~NotifHelper()
{
    delete m_notificationSession;
}

/*!
  Activates Notifications API
*/
void NotifHelper::activate()
{
    qDebug() << "NotifHelper::activate()";

    if (!m_notificationSession)
        initialize();
}


/*!
  Registers Notifications API
  Application goes to online and can invoke API
*/
void NotifHelper::registerApplication(QVariant id)
{
    qDebug() << "NotifHelper::registerApplication()";

    m_cancelled = false;

    if (m_notificationSession) {
        m_notificationSession->registerApplication(id.toString());
        emit busy(true);
    }
}


/*!
  Unregisters Notifications API
  Application goes to offline
*/
void NotifHelper::unregisterApplication()
{
    qDebug() << "NotifHelper::unregisterApplication()";

    if (m_notificationSession) {
        m_notificationSession->unregisterApplication();
        emit busy(true);
    }
}


/*!
  User cancels registration to Notifications API
*/
void NotifHelper::cancel()
{
    qDebug() << "NotifHelper::cancel()";
    m_cancelled = true;
    emit busy(false);

    if (m_notificationSession) {
        m_notificationSession->unregisterApplication();
    }
}


/*!
  Loads Notificatins API service interface
*/
void NotifHelper::initialize()
{
    qDebug() << "NotifHelper::initialize()";
    m_cancelled = false;
    QPluginLoader *loader = new QPluginLoader(ONE_PLUGIN_ABSOLUTE_PATH);

    if (loader) {
        qDebug() << "Plugin loaded";
        QObject *serviceObject = loader->instance();

        if (serviceObject) {
            qDebug() << "Plugin created";

            // Store the service interface for later usage
            m_notificationSession =
                    static_cast<OviNotificationSession*>(serviceObject);

            // Connect signals to slots
            connect(serviceObject, SIGNAL(stateChanged(QObject*)),
                    this, SLOT(changedState(QObject*)));
            connect(serviceObject, SIGNAL(received(QObject*)),
                    this, SLOT(notificationReceived(QObject*)));
        }
        else {
            qDebug() << "Creating plugin failed!";
            emit notificationError();
        }

        delete loader;
    }
}


/*!
  Listens Notifications API states
*/
void NotifHelper::changedState(QObject *aState)
{
    // State of the application has changed
    OviNotificationState *state =
            static_cast<OviNotificationState*>(aState);

    qDebug() << "OviNotificationState to : " << state->sessionState();

    if (m_cancelled) {
        qDebug() << "Cancelled!";
        emit busy(false);
        return;
    }

    // Print out the session state on the screen
    switch (state->sessionState()) {
    case OviNotificationState::EStateOffline: {
        emit busy(false);
        break;
    }
    case OviNotificationState::EStateOnline: {
        // Notifications API is online and activated

        // Set this application to be started when notification arrives
        m_notificationSession->setWakeUp(true);

        // Is user wanted to API be activated or not?
        bool enabled = loadSetting("enabled",false).toBool();

        if (!enabled) {
            // User wants to notification to be disabled
            m_notificationSession->unregisterApplication();
        }
        emit busy(false);
        break;
    }
    case OviNotificationState::EStateConnecting: {
        emit busy(true);
        break;
    }
    default: {
        emit busy(false);
        break;
    }
    }

    if (state->sessionError() != OviNotificationState::EErrorNone) {
        qDebug() << "Error : " << state->sessionErrorString();
        emit busy(false);
    }

    delete state;
}


/*!
  Notification message received from Notifications API
*/
void NotifHelper::notificationReceived(QObject *aNotification)
{
    // Read received notification
    OviNotificationMessage *notification =
            static_cast<OviNotificationMessage*>(aNotification);
    OviNotificationPayload *payload =
            static_cast<OviNotificationPayload*>(notification->payload());

    // Was it alert?
    QString message = payload->dataString();

    if (message.contains("ALERT")) {
        // Do alert!
        emit alarmReceived();
        message = message.right(message.length() - 6);

        // Add to log
        addNewNotification(message);
    }

    delete payload;
    delete notification;
}


/*!
  Add received Notification message to the log
*/
void NotifHelper::addNewNotification(QString receivedMessage)
{
    QDate date = QDate::currentDate();
    QString dateString = date.toString("yyyy-MM-dd");

    QTime time = QTime::currentTime();
    QString timeString = time.toString("HH:mm:ss");

    QString receivedTime = QString("%1 %2").arg(dateString).arg(timeString);

    m_notificationModel->addNotification(receivedTime,receivedMessage);
}


/*!
  Saves a given key - value setting by using QSetting to devices persistent
  storage
*/
void NotifHelper::saveSetting(const QVariant &key, const QVariant &value)
{
    m_settings->setValue(key.toString(), value);
}


/*!
  Loads setting from the devices persistent storage, the key defines the
  setting to load, defaultValue is the value for the key if the value
  not yet exist in the persistent storage
*/
QVariant NotifHelper::loadSetting(const QVariant &key,
                                  const QVariant &defaultValue)
{
    return m_settings->value(key.toString(), defaultValue);
}


/*!
  Getter
*/
NotificationModel* NotifHelper::notificationModel() const
{
    return m_notificationModel;
}


/*!
  Setter
*/
void NotifHelper::setNotificationModel(NotificationModel *notificationModel)
{
    m_notificationModel = notificationModel;
    emit notificationModelChanged();
}
