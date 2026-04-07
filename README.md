# 🦺 Proactive Safety Assessment — PPE Detection System

> *Because human eyes get tired. Ours don't.*

---

## 🔴 Live Status

```
┌─────────────────────────────────────────────┐
│  📷 Camera Feed: ACTIVE                      │
│  🤖 Detection Engine: RUNNING                │
│  🚨 Alert System: ARMED                      │
│  📊 Safety Score: ████████░░  84%            │
└─────────────────────────────────────────────┘
```

---

## What This Is

Every year, thousands of workplace injuries happen not because protocols don't exist — but because someone got tired, got distracted, or simply wasn't looking at the right moment.

This is a computer vision system that watches over industrial work environments **in real time**, detecting missing Personal Protective Equipment (PPE) and flagging violations before they turn into accidents.

It doesn't blink. It doesn't lose focus. It just watches.

---

## 🧩 The Three Pillars

```
 ┌──────────────────────────────────────────────────────────┐
 │  🎥  CAMERA STREAM                                        │
 │       ↓  frame by frame                                   │
 ├──────────────────────────────────────────────────────────┤
 │  🤖  YOLOv8 DETECTION ENGINE                             │
 │       Draws bounding boxes around PPE violations          │
 │       ↓  violation detected                               │
 ├──────────────────────────────────────────────────────────┤
 │  🚨  ALERT ENGINE                                         │
 │       Fires warning via sound / SMS / log                 │
 │       ↓  continuous feed                                  │
 ├──────────────────────────────────────────────────────────┤
 │  📊  SAFETY SCORE DASHBOARD                               │
 │       Live compliance rate — glanceable at any moment     │
 └──────────────────────────────────────────────────────────┘
```

---

## 🦺 PPE We Detect

| Icon | Equipment | Status |
|------|-----------|--------|
| 🪖 | Hard hat / helmet | ✅ Supported |
| 🟡 | High-visibility reflective vest | ✅ Supported |
| 🧤 | Safety gloves | ✅ Supported |
| 😷 | Face mask / respirator | ✅ Supported |
| 👢 | Safety boots | 🔄 In Training |

---

## 🏗️ System Architecture

```
  ┌──────────────────────────────────────────────────────┐
  │                  CAMERA / RTSP FEED                   │
  │          (USB webcam or IP camera stream)             │
  └─────────────────────┬────────────────────────────────┘
                         │  raw frames
                         ▼
  ┌──────────────────────────────────────────────────────┐
  │              DETECTION LAYER                          │
  │                                                       │
  │   detection/                                          │
  │   ├── model/          ← YOLOv8 weights                │
  │   ├── inference.py    ← per-frame inference           │
  │   └── draw_boxes.py   ← bounding box rendering        │
  └─────────────────────┬────────────────────────────────┘
                         │  violation events
                         ▼
  ┌──────────────────────────────────────────────────────┐
  │              ALERT LAYER                              │
  │                                                       │
  │   alerts/                                             │
  │   ├── alert_engine.py ← rolling-window logic          │
  │   └── notifier.py     ← sound / SMS / log             │
  └─────────────────────┬────────────────────────────────┘
                         │  live data
                         ▼
  ┌──────────────────────────────────────────────────────┐
  │              DASHBOARD LAYER                          │
  │                                                       │
  │   dashboard/                                          │
  │   ├── app.py          ← Flask/FastAPI backend          │
  │   ├── safety_score.py ← compliance calculation        │
  │   └── templates/      ← live web UI                   │
  └──────────────────────────────────────────────────────┘
```

---
-------------------

Prerequisites!!!!!!!
#pip install ultralytics roboflow


----------------
## 📁 Project Structure

```
proactive-safety-assessment/
│
├── 📂 detection/
│   ├── 📂 model/              ← YOLOv8 weights + config
│   ├── 🐍 inference.py        ← real-time stream inference
│   └── 🐍 draw_boxes.py       ← bounding box rendering
│
├── 📂 alerts/
│   ├── 🐍 alert_engine.py     ← violation detection logic
│   └── 🐍 notifier.py         ← alert delivery
│
├── 📂 dashboard/
│   ├── 🐍 app.py              ← web dashboard backend
│   ├── 🐍 safety_score.py     ← score calculation
│   └── 📂 templates/          ← frontend HTML/JS
│
├── 📂 data/
│   └── 📂 sample_footage/     ← test clips for local dev
│
├── 📄 requirements.txt
└── 📄 README.md
```

---

## 🗓️ Project Schedule

```
WEEK  MILESTONE                              MODULE        STATUS
───────────────────────────────────────────────────────────────────
day1  Project setup & dataset collection    Foundation    ✅ Done
      ↳ Environment, camera wiring,
        500 labeled training images

  YOLOv8 model training & validation    Detection     ✅ Done
      ↳ Fine-tune, mAP evaluation,
        hyperparameter iteration

 Real-time inference + bounding boxes  Detection     ⟳ Active
      ↳ Live stream → box overlay → labels

 Alert engine build                    Alerts        ⟳ Active
      ↳ Rolling-window logic, SMS tests,
        threshold configuration

day2  Dashboard MVP                         Dashboard     ○ Upcoming
      ↳ Live safety score, violation log,
        real-time compliance chart

day2  System integration & stress testing   Integration   ○ Upcoming
      ↳ All layers end-to-end, latency
        benchmarks, edge case handling

day3  Demo prep & submission polish         MVP           ○ Upcoming
      ↳ Demo video, README final,
        live presentation walkthrough
───────────────────────────────────────────────────────────────────

PROGRESS  [██████████░░░░░░░░░░░░░░]  3day to complete
```

---

## ⚡ Quick Start

### Prerequisites

- Python 3.9+
- A webcam or RTSP camera stream
- GPU recommended (CPU works for testing)

### Install

```bash
git clone https://github.com/your-team/proactive-safety-assessment.git
cd proactive-safety-assessment
pip install -r requirements.txt
```

### Run detection

```bash
# On your webcam
python detection/inference.py --source 0

# On a video file
python detection/inference.py --source data/sample_footage/test_clip.mp4
```

### Launch dashboard

```bash
python dashboard/app.py
# Open http://localhost:5000
```

---

## 📊 How the Safety Score Works

```
  Compliant Detections
 ─────────────────────── × 100  =  Safety Score
   Total Detections

  Example:
  8 workers detected → 2 missing PPE → 6 compliant

   6
  ─── × 100  =  75  →  🟡 Warning threshold
   8
```

The score rolls over a configurable window (default: last 60 seconds). If it drops below a set threshold for more than 30 consecutive seconds, an escalation alert fires.

---

## 🚨 Alert Thresholds

```
Score     Status         Action
────────────────────────────────────────────
90–100    🟢 Safe        No action
70–89     🟡 Caution     Log + soft alert
50–69     🟠 Warning     Sound alarm + notify supervisor
< 50      🔴 Critical    Escalation: SMS + access log flag
```

---

## ⚠️ Honest Caveats

This is an MVP — it works, but it's not perfect.

- **Lighting**: Model performs best in well-lit, unobstructed scenes. Dark or cluttered frames reduce accuracy.
- **Angles**: Unusual camera positions (top-down, extreme side angles) can affect confidence scores.
- **False positives**: There is a baseline rate. The rolling-window alert logic is tuned to minimize these, but not eliminate them entirely.

We'd recommend treating this as a second set of eyes that catches what a human might miss — not as a replacement for trained safety personnel.

---

## 🔭 What's Next

- Multi-camera support with a unified dashboard view
- Zone-based monitoring (different areas, different PPE requirements)
- Historical reporting and trend analysis over shifts
- Integration with site access control and HR systems

---

## 👥 The Team

Built for the **Proactive Safety Assessment Challenge**. We believe the best use of computer vision is making the people who work in hard, physical environments a little safer every day.

Questions, feedback, or collaboration? Open an issue or reach out directly.

---


---

*YOLOv8 · Python · OpenCV · Flask · Challenge Submission*
