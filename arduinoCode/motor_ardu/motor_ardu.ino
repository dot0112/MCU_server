#include <SPI.h>
#include <mcp_can.h>

#include <ServoTimer2.h> 
#define CANPin 8  // CAN 버스 핀
#define motor1_A 10 // 모터1 A핀
#define motor1_B 9  // 모터1 B 핀
#define motor2_A 6  // 모터2 A핀
#define motor2_B 5  // 모터2 B핀
#define motorspeed 150  // 모터 속도
#define servoPin 3  // servo 모터 핀
#define MaxAngle 2000 // servo 모터 최대 각도
#define MinAngle 1000 // servo 모터 최소 각도
#define changeAngleVal 100 // 1 회당 변경 각도

// CAN Bus용 변수
unsigned long mask = 0x77;
unsigned long filter = 0x01000015;
MCP_CAN CAN(CANPin);

// Serial 통신 세팅
char flag;
int servoval = 1500;
ServoTimer2 myServo;

void setup() {
  // Serial 통신 세팅
  Serial.begin(9600);
  // CAN Bus 세팅
  CAN.begin(MCP_ANY, CAN_1000KBPS, MCP_8MHZ);
  CAN.setMode(MCP_NORMAL);
  CAN.init_Mask(0, 0, mask);
  CAN.init_Filt(0, 0, filter);
  // servo 모터 세팅
  pinMode(motor1_A, OUTPUT);
  pinMode(motor1_B, OUTPUT);
  pinMode(motor2_A, OUTPUT);
  pinMode(motor2_B, OUTPUT);
  myServo.attach(servoPin);
  myServo.write(servoval);

  digitalWrite(motor1_A, LOW);
  digitalWrite(motor1_B, LOW);
  digitalWrite(motor2_A, LOW);
  digitalWrite(motor2_B, LOW);
}

void loop() {
  if (CAN_MSGAVAIL == CAN.checkReceive()) {
    long unsigned int rxId;
    unsigned char len = 0;
    unsigned char rxBuf[8];

    CAN.readMsgBuf(&rxId, &len, rxBuf);

    for (int i = 0; i < len; i++) {
      flag = (char)rxBuf[i];
      servoval = myServo.read();
      OP(flag, servoval);
      delay(100);
      OP(' ', NULL);
    }
  }
}

void OP(char c, int SVval){
  switch(c){
    case 'w':
      forward();
      break;
    case 's':
      backward();
      break;
    case 'a':
      leftward(SVval);
      break;
    case 'd':
      rightward(SVval);
      break;
    default:
      stop();  
  }
}

void forward()
{
  Serial.println("forward");
analogWrite(motor1_A, motorspeed);
analogWrite(motor1_B, 0);
analogWrite(motor2_A, motorspeed);
analogWrite(motor2_B, 0);
}
void backward()
{
  Serial.println("backward");
  analogWrite(motor1_A, 0);
  analogWrite(motor1_B, motorspeed);
  analogWrite(motor2_A, 0);
  analogWrite(motor2_B, motorspeed);
}
void leftward(int val)
{
  Serial.println("leftward");
  if (val > MinAngle) {
        val -= changeAngleVal;
        myServo.write(val);
      }
}
void rightward(int val)
{
  Serial.println("rightward");
  if (val < MaxAngle) {
        val += changeAngleVal;
        myServo.write(val);
      }
}
void stop()
{
  analogWrite(motor1_A, 0);
  analogWrite(motor1_B, 0);
  analogWrite(motor2_A, 0);
  analogWrite(motor2_B, 0);
}