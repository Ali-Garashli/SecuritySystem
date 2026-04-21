#include <Wire.h>
#include <LiquidCrystal_I2C.h>

// constants
const int MQ2_PIN    = A0;
const int FLAME_PIN  = 8;
const int PIR_PIN    = 7;
const int BUZZER_PIN = 9;
const int GAS_DANGER_LEVEL = 300;
const unsigned long SEND_INTERVAL_MS = 500; // interval of sending data to bridge

// lcd display
LiquidCrystal_I2C lcd(0x27, 16, 2);

// globals
bool systemArmed = true; // updated by Bridge (A:)
bool buzzerServerActive = true; // updated by bridge (B:)
unsigned long lastSendMs = 0;

// Pending incoming data from bridge
String bridgeBuffer = "";

void setup() {
    Serial.begin(9600);

    pinMode(FLAME_PIN, INPUT);
    pinMode(PIR_PIN, INPUT);
    pinMode(BUZZER_PIN, OUTPUT);
    digitalWrite(BUZZER_PIN, LOW);

    lcd.init();
    lcd.backlight();
    lcd.setCursor(0, 0);
    lcd.print("System Starting");
    delay(2000);
    lcd.clear();
}

void loop() {
    unsigned long now = millis();

    // READ SENSORS
    int gasAdc = analogRead(MQ2_PIN);
    int flameRaw = digitalRead(FLAME_PIN); // LOW (0) = flame
    int motionRaw = digitalRead(PIR_PIN); // HIGH (1) = motion

    // Convert to values the API (SensorCalculator) understand:
    // Flame:  0 = dangerous, 1023 = safe
    // Motion: 0 = safe, 1023 = dangerous
    int flameValue = (flameRaw  == LOW) ? 0 : 1023;
    int motionValue = (motionRaw == HIGH) ? 1023 : 0;

    // LOCAL ALERTS
    bool gasAlert = (gasAdc >= GAS_DANGER_LEVEL);
    bool flameAlert = (flameValue == 0);
    bool motionAlert = (motionValue != 0);
    bool anyAlert = gasAlert || flameAlert || motionAlert;

    // BUZZER
    // When the bridge is connected, use the server's decision (buzzerServerActive).
    // This ensures disarming from the web UI silences the buzzer immediately.
    // If the bridge has never replied yet (standalone / offline), fall back to
    // local sensor detection so the buzzer still works without a network.
    bool buzzerOn = bridgeConnected ? (buzzerServerActive && anyAlert) : anyAlert;
    digitalWrite(BUZZER_PIN, buzzerOn ? HIGH : LOW);

    // LCD
    // line 0 for gas reading and isArmed state
    lcd.setCursor(0, 0);
    lcd.print("Gas:");
    lcd.print(gasAdc);
    // add padding
    int digits = (gasAdc == 0) ? 1 : (int)log10((double)gasAdc) + 1;
    for (int i = digits; i < 4; i++) lcd.print(' ');

    // armed indicator on same line, right side
    lcd.setCursor(11, 0);
    lcd.print(systemArmed ? "[ARMED]" : "       ");

    // line 1 for alert status
    lcd.setCursor(0, 1);
    switch (true) {
        case gasAlert && flameAlert:
            lcd.print("CRITICAL FIRE!  ");
            break;
        case gasAlert:
            lcd.print("Gas Detected    ");
            break;
        case flameAlert:
            lcd.print("Flame Detected  ");
            break;
        case motionAlert:
            lcd.print("Motion Alert!   ");
            break;
        default:
            lcd.print("System Normal   ");
            break;
    }

    // SEND TO BRIDGE
    if (now - lastSendMs >= SEND_INTERVAL_MS) {
        lastSendMs = now;

        // format: G:123,F:1023,M:0
        Serial.print("G:");
        Serial.print(gasAdc);
        Serial.print(",F:");
        Serial.print(flameValue);
        Serial.print(",M:");
        Serial.println(motionValue);
    }

    // READ FROM BRIDGE
    // A:0 or A:1 for armed state (ro display on lcd)
    // B:0 or B:1 for buzzer override (0 = turn off, 1 = use local logic)
    while (Serial.available()) {
        char c = (char)Serial.read();
        if (c == '\n') {
            bridgeBuffer.trim();
            if (bridgeBuffer.startsWith("A:")) {
                systemArmed = (bridgeBuffer.charAt(2) == '1');
            } else if (bridgeBuffer.startsWith("B:")) {
                buzzerServerActive = (bridgeBuffer.charAt(2) == '1');
                bridgeConnected = true; // bridge has spoken at least once
            }
            bridgeBuffer = "";
        } else {
            bridgeBuffer += c;
        }
    }

    delay(50); // small delay to avoid tight-spinning
}
