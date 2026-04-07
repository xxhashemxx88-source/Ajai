import * as signalR from '@microsoft/signalr'
import { BASE_URL } from './api'

let connection = null

/**
 * ينشئ اتصال SignalR ويبدأ الاستماع للأحداث
 * @param {object} handlers - { onNewAlert, onCameraStatusChanged, onCameraCommand }
 */
export async function startSignalR(handlers = {}) {
  if (connection?.state === signalR.HubConnectionState.Connected) return connection

  const token = localStorage.getItem('ajai_token')

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${BASE_URL}/dashboardHub`, {
      accessTokenFactory: () => token
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  // استقبال فريم حي من الكاميرا (buffer - يُستبدل)
  if (handlers.onLiveFrame) {
    connection.on('ReceiveLiveFrame', handlers.onLiveFrame)
  }

  // استقبال تنبيه جديد (يُضاف للقائمة)
  if (handlers.onNewAlert) {
    connection.on('ReceiveNewAlert', handlers.onNewAlert)
  }

  // استقبال تغيير حالة الكاميرا
  if (handlers.onCameraStatusChanged) {
    connection.on('CameraStatusChanged', handlers.onCameraStatusChanged)
  }

  // استقبال أمر للكاميرا (START/STOP STREAM)
  if (handlers.onCameraCommand) {
    connection.on('ReceiveCameraCommand', handlers.onCameraCommand)
  }

  // إعادة الانضمام تلقائياً عند إعادة الاتصال
  connection.onreconnected(async () => {
    console.log('[SignalR] Reconnected')
    if (handlers.onReconnected) handlers.onReconnected()
  })

  // انقطع الاتصال نهائياً بعد كل المحاولات → logout
  connection.onclose(() => {
    console.log('[SignalR] Connection closed permanently')
    if (handlers.onDisconnected) handlers.onDisconnected()
  })

  await connection.start()
  console.log('[SignalR] Connected')

  return connection
}

/**
 * ينضم لمجموعة Admins (للمديرين)
 */
export async function joinAdminGroup() {
  if (connection) {
    await connection.invoke('JoinDashboard')
  }
}

/**
 * ينضم لمجموعة الكاميرا (للكاميرات)
 */
export async function joinCameraGroup(email) {
  if (connection) {
    await connection.invoke('JoinCameraGroup', email)
  }
}

export async function stopSignalR() {
  if (connection) {
    await connection.stop()
    connection = null
  }
}

export function getConnection() {
  return connection
}