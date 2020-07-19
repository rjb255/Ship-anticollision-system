const String PersonalReference = "000-000-000";
int MessageReference = "0";
//Target, Interval, Distance, AT_TIME, Last_SENT, Vel 
char *targets0[4]; char *targets1[4]; double targets2[4]; int targets3[4]; int targets4[4]; double targets5[4];
//Target, recieve/transmit, timestamp, distance measured, dd/dt
char *messages0[50]; char *messages1[50]; int messages2[50]; double messages3[50]; double messages4[50];
String incoming;
int targLen = 1; int messLen = 0;
int c = 3.43; //sos

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  targets0[0] = "ALL"; targets1[0] = 10000; targets4[0] = 0;
  
}

void loop() {
  // put your main code here, to run repeatedly:
  if (messLen > 40){
    for (int i = 20; i < 50; i++){
      messages0[i - 20] = messages0[i]; messages1[i - 33] = messages1[i]; messages2[i - 33] = messages2[i]; messages3[i - 33] = messages3[i]; messages4[i - 33] = messages4[i];
      messLen -= 20;
    }
  }
  for(int i = 0; i < targLen; i++){
    
    if (millis() - targets4[i] > targets1[i]){
      
      serial_out(targets0[i], PersonalReference + "/" + String(MessageReference), "General", String(millis()), String(targets1[i]), String(targets2[i]));
      targets4[i] = millis();
      
      messages0[messLen] = targets0[i]; messages1[messLen] = "trasmit"; messages2[messLen] = targets4[i];
      messLen++;
      MessageReference++;
    }
  }
  
  
  
  
  if (Serial.read() > 0){
    incoming += Serial.readString();
    if (incoming.indexOf("SEASWAYCOLLISIONTHERAPY",0) > 0){
      incoming.remove(0,incoming.indexOf("SEASWAYCOLLISIONTHERAPY",0));
    };
    if (incoming.indexOf("AHOYMILADIES",0) > 0){
      
      int targ[] = {incoming.indexOf("TO: ",0) + 4, 0};
      targ[1] = incoming.indexOf("_",targ[0]);
      String to = (incoming.substring(targ[0],targ[1]));
      
      if (to == "ALL" || to == PersonalReference){
        to.toCharArray(messages0[messLen], 11); messages1[messLen] = "recieve"; messages2[messLen] = (millis());
        int from[] = {incoming.indexOf("FROM: ",0) + 6, 0};
        from[1] = from[0] + 11;
        String via = incoming.substring(from[0],from[1]);
        
        via.toCharArray(messages0[messLen],11);
        messLen++;
        bool known = false;
        for (int i = 0; i < targLen; i++){
          if (targets0[i] == messages0[messLen - 1]){
            known = true;
          }
        }
        if (known == false){
          via.toCharArray(targets0[targLen],11);
          targets1[targLen] = 300;
          targLen++;
        }
        
        int typ[] = {incoming.indexOf("TRANSMISSIONTYPE: ",0) + 18, 0};
        typ[1] = incoming.indexOf("_",typ[0]);
        String top = incoming.substring(typ[0],typ[1]);
        
        if (top != "General"){
          int i = messLen;
          bool found = false;
          while (i > 0 && found == false){
            i--;
            if (String(messages0[i]) == via){ found = true; }
          }
          messages3[messLen - 1] = (millis() - messages2[i]) * c / 2;
        }
        
        if (top != "REPLY1"){
          String typical = (top == "General") ? "REPLY0" : "REPLY1";
          serial_out(via, PersonalReference + "/" + String(MessageReference), typical, String(millis()), "", "");
        }
      }
      incoming.remove(0,incoming.indexOf("AHOYMILADIES",0));
    }
  }

}

void serial_out(String to, String from, String type, String Time, String interval, String dist) {
  /*
  SEASWAYCOLLISIONTHERAPY
  TO: [ALL,REFERENCE]
  FROM: 000-000-001/x
  Transmission_Type: [General/REPLY0/REPLY1]
  TRANSMISSION-TIME: millis
  TRANSMISSION INTERVAL: []
  MEASURED DISTANCE: [N/A,xxxx]
  AHOYMILADIES
  */
  String msg = 
            "SEASWAYCOLLISIONTHERAPY_"
            "TO: " + to + "_"
            "FROM: " + from + "_"
            "TRANSMISSIONTYPE: " + type + "_"
            "TRANSMISSIONTIME: " + Time + "_"
            "TRANSMISSIONINTERVAL: " + interval + "_"
            "MEASURED DISTANCE: " + dist + "_"
            "AHOYMILADIES";
  Serial.print("||Here||");
  Serial.print(msg);
}