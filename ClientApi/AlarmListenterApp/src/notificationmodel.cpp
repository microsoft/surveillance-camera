/**
 * Copyright (c) 2012 Nokia Corporation.
 */

#include "notificationmodel.h"

#include <qdeclarative.h>
#include <QFile>


/*!
  \class NotificationData
  \brief Custom QML data element to transfer notification information to
         the QML UI. Class has also stream operators to allow serialization
         of NotificationData object.
*/


/*!
  Constructor, makes empty notification data.
*/
NotificationData::NotificationData()
    : m_receivedTimestamp(""),
      m_sendTimestamp("")
{
}


/*!
  Constructor, initializes ScoreData.
*/
NotificationData::NotificationData(const QString &receivedTimestamp,
                                   const QString &sendTimestamp)
    : m_receivedTimestamp(receivedTimestamp),
      m_sendTimestamp(sendTimestamp)
{
}


/*!
  Copy constructor.
*/
NotificationData::NotificationData(const NotificationData &notificationData)
    : QObject(),
      m_receivedTimestamp(notificationData.m_receivedTimestamp),
      m_sendTimestamp(notificationData.m_sendTimestamp)
{
}


/*!
  Assigment operator.
*/
NotificationData& NotificationData::operator=(const NotificationData &notificationData)
{
    m_receivedTimestamp = notificationData.m_receivedTimestamp;
    m_sendTimestamp = notificationData.m_sendTimestamp;

    return *this;
}


/*!
  Received timestamp get method.
*/
QString NotificationData::receivedTimestamp() const
{
    return m_receivedTimestamp;
}


/*!
  Send timestamp get method.
*/
QString NotificationData::sendTimestamp() const
{
    return m_sendTimestamp;
}


/*!
  Stream operator to place NotificationData to the given stream.
*/
QDataStream& operator<<(QDataStream &stream, const NotificationData &data)
{
    return stream << data.receivedTimestamp() << data.sendTimestamp();
}


/*!
  Stream operator to get the NotificationData from given stream.
*/
QDataStream& operator>>(QDataStream &stream, NotificationData &data)
{
    QString receivedTimestamp;
    QString sendTimestamp;

    stream >> receivedTimestamp >> sendTimestamp;

    data = NotificationData(receivedTimestamp, sendTimestamp);

    return stream;
}




/*!
  \class NotificationModel
  \brief Custom list model to provide notification information to QML.
*/

/*!
  Constructor, sets the roles for data. QML will query data with these roles.
*/
NotificationModel::NotificationModel(QObject *parent)
    : QAbstractListModel(parent)
{
    QHash<int, QByteArray> roles;
    roles[ReceivedTimestampRole] = "receivedTimestamp";
    roles[SendTimestampRole] = "sendTimestamp";
    setRoleNames(roles);

    m_file.setFileName("notifications.dat");

    if (m_file.open(QIODevice::ReadWrite)) {
        m_dataStream.setDevice(&m_file);

        QList<NotificationData> notifications;
        NotificationData data;

        while (!m_dataStream.atEnd()) {
            m_dataStream >> data;
            notifications.insert(0, data);
        }

        if (notifications.count()) {
            beginInsertRows(QModelIndex(), 0, notifications.count() - 1);
            m_notifications = notifications;
            endInsertRows();
        }
    }
}


/*!
  Adds given notification to as first notification in notifications list.
*/
void NotificationModel::addNotification(const QString &receivedTimestamp,
                                        const QString &sendTimestamp)
{
    beginInsertRows(QModelIndex(), 0, 0);
    m_notifications.insert(0, NotificationData(receivedTimestamp,
                                               sendTimestamp));
    endInsertRows();

    m_dataStream << m_notifications.first();
}


/*!
  Returns the row count.
*/
int NotificationModel::rowCount(const QModelIndex &parent) const
{
    Q_UNUSED(parent);
    return m_notifications.count();
}


/*!
  Returns the data in given row and role.
*/
QVariant NotificationModel::data(const QModelIndex &index, int role) const
{
    if (index.row() < 0 || index.row() > m_notifications.count())
        return QVariant();

    const NotificationData &notification = m_notifications[index.row()];
    if (role == ReceivedTimestampRole)
        return notification.receivedTimestamp();
    else if (role == SendTimestampRole)
        return notification.sendTimestamp();

    return QVariant();
}


/*!
  Clears the model and sets the file content size to 0.
*/
void NotificationModel::clear()
{
    if (m_notifications.empty()) {
        return;
    }

    beginRemoveRows(QModelIndex(), 0, m_notifications.count()-1);
    m_notifications.clear();
    endRemoveRows();

    m_dataStream.setDevice(0);
    m_file.resize(0);
    m_dataStream.setDevice(&m_file);

}


QML_DECLARE_TYPE(NotificationModel)
