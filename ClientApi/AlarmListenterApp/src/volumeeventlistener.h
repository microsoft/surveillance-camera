/**
 * Copyright (c) 2012 Nokia Corporation.
 */

#ifndef VOLUMEEVENTLISTENER_H
#define VOLUMEEVENTLISTENER_H

#include <QObject>
#include <remconcoreapitargetobserver.h>

class CRemConInterfaceSelector;
class CRemConCoreApiTarget;


class VolumeEventListener : public QObject, public MRemConCoreApiTargetObserver
{
    Q_OBJECT

public:
    explicit VolumeEventListener(QObject *parent = 0);
    virtual ~VolumeEventListener();

public:
    void MrccatoCommand(TRemConCoreApiOperationId aOperationId,
                        TRemConCoreApiButtonAction aButtonAct);

signals:
    void volumeUp();
    void volumeDown();

private:
    CRemConInterfaceSelector *m_Selector;
    CRemConCoreApiTarget *m_Target;
};

#endif // VOLUMEEVENTLISTENER_H


