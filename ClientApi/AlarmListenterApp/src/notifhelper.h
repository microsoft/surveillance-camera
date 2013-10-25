#ifndef NOTIFHELPER_H
#define NOTIFHELPER_H

#include <QObject>
#include <QVariant>

class QSettings;
class OviNotificationSession;
class NotificationModel;


class NotifHelper : public QObject
{
    Q_OBJECT
    Q_PROPERTY(NotificationModel* notificationModel
               READ notificationModel WRITE setNotificationModel
               NOTIFY notificationModelChanged)

public:
    explicit NotifHelper(QObject *parent = 0);
    ~NotifHelper();

public slots:
    void activate();
    void registerApplication(QVariant id);
    void unregisterApplication();
    void saveSetting(const QVariant &key, const QVariant &value);
    QVariant loadSetting(const QVariant &key, const QVariant &defaultValue);
    void cancel();

private:
    void initialize();
    NotificationModel *notificationModel() const;
    void setNotificationModel(NotificationModel *notificationModel);
    void addNewNotification(QString receivedMessage);

private slots:
    void changedState(QObject*);
    void notificationReceived(QObject*);

signals:
    void alarmReceived();
    void notificationModelChanged();
    void busy(bool isBusy);
    void notificationError();

private:
    OviNotificationSession *m_notificationSession; // Owned
    NotificationModel *m_notificationModel; // Not owned
    QSettings *m_settings; // Owned
    bool m_cancelled;
};

#endif // NOTIFHELPER_H
