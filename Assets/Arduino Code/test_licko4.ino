
int val=0;
int reward=0;
int reward_duration=0;
int random_pulse_duration;
int lick_duration;
int switch_give_reward;
int receivedValue;
unsigned long toff;
unsigned long toff_random_pulse;

unsigned long tnow;
unsigned long timestamp;
String outString="";

bool inPulse = false;
bool inRandomPulse = false;

bool switchPressed =false;

//char inp='0';

void setup() {
  // put your setup code here, to run once:
pinMode(13, INPUT); // Joystick or Lickoswitch input
//pinMode(12, INPUT);
pinMode(14, INPUT_PULLUP); //External Reward Switch
pinMode(11, OUTPUT); // 
pinMode(12, OUTPUT); //Random Pulse

Serial.begin(115200);

digitalWrite(11,LOW);

}

void loop() {
  // put your main code here, to run repeatedly:
  
  
  char bb='n';
  int len = Serial.available();
  if (len!=0)
  {
    char buf[len];
    for (int i=0; i<len;i++)
      buf[i]=Serial.read(); 
    receivedValue = atoi(buf); 
    if (receivedValue>=0)
    {
      bb='y';
      reward_duration = receivedValue;
    }
    else
    {
      bb='r';
      random_pulse_duration = -1 * receivedValue;
    }
    
    
  }
  
  //Check the switch - switch operates as "Give reward" 
  //if (!switchPressed && digitalRead(14)==1)
  //{
  //  if (reward_duration==0) 
  //      reward_duration=160;
  //
  //  switchPressed = true;
  //  bb='y';
  //}
  //if (switchPressed && digitalRead(14)==0)
  //    switchPressed = false;

  //Check the switch - switch operates like the external foot switches
  if (digitalRead(14)==LOW)
    digitalWrite(11,HIGH);
  else 
    if (!inPulse)
      digitalWrite(11,LOW);

  
  if (bb=='y')
  {
    toff = millis() + reward_duration;
    digitalWrite(11,HIGH);
    inPulse = true;
    //inp='1';
  }

  if (inPulse)
  {
    tnow = millis();
    if (millis()>toff)
    {
      digitalWrite(11,LOW);
      inPulse=false;
      //inp='0';
    }
  }


  if (bb=='r')
  {
      toff_random_pulse = millis() + random_pulse_duration;
      digitalWrite(12,HIGH);
      inRandomPulse = true;
  }
  if (inRandomPulse)
  {
    tnow = millis();
    if (millis()>toff_random_pulse)
    {
      digitalWrite(12,LOW);
      inRandomPulse=false;
      //inp='0';
    }
  }
  
  
  val = digitalRead(13);   // read the input pin
  //reward = digitalRead(12); // read the give reward signal
 
  timestamp = millis();
  //outString = (String) timestamp+","+(String) val + "," + (String) reward + ","+bb+","+inp; 
  //outString = (String) timestamp+","+(String) val + "," + (String) reward; 
  //outString = (String) timestamp+","+(String)reward_duration  + "," + (String) random_pulse_duration; 
  
  //>>> 
  outString = (String) timestamp+","+(String) val ; 
  Serial.println(outString);
  Serial.flush();
  //Serial.println((String)len+","+(String)lick_duration);
  delay(10);
}
