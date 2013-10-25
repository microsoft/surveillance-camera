/**
 * Copyright (c) 2012 Nokia Corporation.
 */

#ifndef NOTIFICATIONMODEL_H
#define NOTIFICATIONMODEL_H

#include <QAbstractListModel>
#include <QDataStream>
#include <QFile>


class NotificationData : public QObject
{
    Q_OBJECT

public:
    NotificationData();
    NotificationData(const QString &receivedTimestamp,
                     const QString &sendTimestamp);
    NotificationData(const NotificationData &notificationData);
    NotificationData& operator=(const NotificationData &notificationData);

    QString receivedTimestamp() const;
    QString sendTimestamp() const;

protected:
    QString m_receivedTimestamp;
    QString m_sendTimestamp;
};

QDataStream& operator<<(QDataStream &stream, const NotificationData &data);
QDataStream& operator>>(QDataStream &stream, NotificationData &data);


class NotificationModel : public QAbstractListModel
{
    Q_OBJECT

public:
    enum NotificationRoles {
        ReceivedTimestampRole = Qt::UserRole + 1,
        SendTimestampRole
    };

    NotificationModel(QObject *parent = 0);

    void addNotification(const QString &receivedTimestamp,
                         const QString &sendTimestamp);

    int rowCount(const QModelIndex &parent = QModelIndex()) const;
    QVariant data(const QModelIndex &index, int role = Qt::DisplayRole) const;

    Q_INVOKABLE void clear();

protected:
    QList<NotificationData> m_notifications;
    QFile m_file;
    QDataStream m_dataStream;
};


#endif // NOTIFICATIONMODEL_H
