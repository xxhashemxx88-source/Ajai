import cv2
import numpy as np
import base64
from flask import Flask, request, jsonify
from ultralytics import YOLO

app = Flask(__name__)

#  NCNN 
MODEL_PATH = "yolo11_ncnn_model"
model = YOLO(MODEL_PATH, task="detect")
model.overrides["verbose"] = False

CONFIDENCE_THRESHOLD = 0.70
DANGER_KEYWORDS = ["no", "without", "bare", "head", "missing"]

@app.route('/analyze', methods=['POST'])
def analyze_frame():
    try:
        # 1  
        data = request.get_json()
        if not data or 'base64Image' not in data:
            return jsonify({"error": "Missing base64Image"}), 400

        base64_str = data['base64Image']
        
        if "," in base64_str:
            base64_str = base64_str.split(",")[1]

        img_data = base64.b64decode(base64_str)
        np_arr = np.frombuffer(img_data, np.uint8)
        frame = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)

        if frame is None:
            return jsonify({"error": "Invalid image format"}), 400

        # 3.  YOLO
        results = model.predict(frame, conf=CONFIDENCE_THRESHOLD, verbose=False)
        annotated_frame = frame.copy()
        
        is_threat = False
        threat_names = []

        if results[0].boxes is not None:
            boxes = results[0].boxes.xyxy.cpu().tolist()
            confs = results[0].boxes.conf.cpu().tolist()
            class_ids = results[0].boxes.cls.int().cpu().tolist()

            for box, conf, cls_id in zip(boxes, confs, class_ids):
                x1, y1, x2, y2 = map(int, box)
                class_name = model.names[cls_id].lower()

                is_violating = any(kw in class_name for kw in DANGER_KEYWORDS)

                if is_violating:
                    is_threat = True
                    if class_name not in threat_names:
                        threat_names.append(class_name)
                    color = (0, 0, 255) # أحمر للتهديد
                else:
                    color = (0, 255, 0) # أخضر للأمان

                cv2.rectangle(annotated_frame, (x1, y1), (x2, y2), color, 2)
                label = f"{class_name} {conf:.2f}"
                (tw, th), _ = cv2.getTextSize(label, cv2.FONT_HERSHEY_SIMPLEX, 0.6, 1)
                cv2.rectangle(annotated_frame, (x1, y1 - th - 10), (x1 + tw + 10, y1), color, -1)
                cv2.putText(annotated_frame, label, (x1 + 5, y1 - 5), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 255), 2)

        # 4.  
        _, buffer = cv2.imencode('.jpg', annotated_frame, [cv2.IMWRITE_JPEG_QUALITY, 70])
        annotated_base64 = base64.b64encode(buffer).decode('utf-8')

        # 5. 
        final_threat_name = ", ".join(threat_names) if is_threat else ""

        response_payload = {
            "isThreat": is_threat,
            "threatName": final_threat_name,
            "annotatedImage": annotated_base64
        }

        return jsonify(response_payload), 200

    except Exception as e:
        print(f"Error processing frame: {e}")
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':

    print("🚀 بدء تشغيل خادم الذكاء الاصطناعي (AI Worker)...")
    app.run(host='0.0.0.0', port=8000, debug=False)
