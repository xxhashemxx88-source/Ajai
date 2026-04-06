import cv2
from ultralytics import YOLO

model = YOLO("best.pt")

cap = cv2.VideoCapture(0)

CONFIDENCE_THRESHOLD = 0.60
REQUIRED_FRAMES = 5

track_history = {}           

DANGER_KEYWORDS = ["no", "without", "bare", "head"]


while cap.isOpened():
    success, frame = cap.read()
    if not success:
        break

    results = model.track(frame, persist=True, conf=CONFIDENCE_THRESHOLD, verbose=False)

    if results[0].boxes is not None and results[0].boxes.id is not None:
        
        boxes = results[0].boxes.xyxy.cpu().tolist()
        track_ids = results[0].boxes.id.int().cpu().tolist()
        confs = results[0].boxes.conf.cpu().tolist()
        class_ids = results[0].boxes.cls.int().cpu().tolist()

        for box, track_id, conf, cls_id in zip(boxes, track_ids, confs, class_ids):
            x1, y1, x2, y2 = map(int, box)
            
            class_name = model.names[cls_id].lower()
            
            track_history[track_id] = track_history.get(track_id, 0) + 1

            if track_history[track_id] >= REQUIRED_FRAMES:
                
                is_unsafe = any(keyword in class_name for keyword in DANGER_KEYWORDS)

                if is_unsafe:
                    color = (0, 0, 255)
                    status = "DANGER"
                else:
                    color = (0, 255, 0)
                    status = "SAFE"

                cv2.rectangle(frame, (x1, y1), (x2, y2), color, 2)
                text = f"{status}: {class_name} | {conf*100:.0f}%"
                cv2.putText(frame, text, (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.6, color, 2)
                
            else:
                cv2.rectangle(frame, (x1, y1), (x2, y2), (0, 255, 255), 2)
                text = f"Checking {class_name}... {track_history[track_id]}/{REQUIRED_FRAMES}"
                cv2.putText(frame, text, (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 255), 2)

    cv2.imshow("Hack4Future - Smart PPE Tracker", frame)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()