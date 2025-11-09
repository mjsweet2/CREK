 
 
"""
    this is a mini server to run rl skills remotely
        
        

        

"""

# pip install --force-reinstall numpy==1.26.4


import sys
sys.path.append('c:\\crek\\python\\')




##########################################################################################
# stuff for service

##########################################################################################

import skillrigdescriptor as rig


##########################################################################################
# stuff for mqtt

##########################################################################################


import random
import time
import json

from paho.mqtt import client as mqtt_client

##########################################################################################
# stuff for pytorch

##########################################################################################



import copy

import pygame

import mujoco
import mujoco.viewer
import zxyedemiquad_rolltofaceup 
import zxyedemiquad_rolltofacedown
import zxyedemiquad_fdfw 
import zxyedemiquad_fdbw 
import zxyedemiquad_fdlt
import zxyedemiquad_fdrt

import numpy as np
import matplotlib.pyplot as plt
import pickle

import sac_comp as sac

import torch
import torch.nn as nn
import torch.optim as optim
import torch.nn.functional as F
from torch.distributions import Normal

import datetime



skillrig_descs = {}

skillrig_descs['rolltofaceup'] = rig.SkillRigDescriptor()
skillrig_descs['rolltofaceup'].loadFromJSON('rolltofaceup')
skillrig_descs['rolltofacedown'] = rig.SkillRigDescriptor()
skillrig_descs['rolltofacedown'].loadFromJSON('rolltofacedown')

skillrig_descs['fdfw'] = rig.SkillRigDescriptor()
skillrig_descs['fdfw'].loadFromJSON('fdfw')
skillrig_descs['fdbw'] = rig.SkillRigDescriptor()
skillrig_descs['fdbw'].loadFromJSON('fdbw')

skillrig_descs['fdlt'] = rig.SkillRigDescriptor()
skillrig_descs['fdlt'].loadFromJSON('fdlt')
skillrig_descs['fdrt'] = rig.SkillRigDescriptor()
skillrig_descs['fdrt'].loadFromJSON('fdrt')






machines = {}

machines['rolltofaceup'] = zxyedemiquad_rolltofaceup.ZXYEDemiquadRollToFaceUpNonSensor()
machines['rolltofacedown'] = zxyedemiquad_rolltofacedown.ZXYEDemiquadRollToFaceDownNonSensor()

machines['fdfw'] = zxyedemiquad_fdfw.ZXYEDemiquadFDFWNonSensor()
machines['fdbw'] = zxyedemiquad_fdbw.ZXYEDemiquadFDBWNonSensor()

machines['fdlt'] = zxyedemiquad_fdlt.ZXYEDemiquadFDLTNonSensor()
machines['fdrt'] = zxyedemiquad_fdrt.ZXYEDemiquadFDRTNonSensor()






agents = {}

agents['rolltofaceup'] = sac.SACComponentsAgent(machines['rolltofaceup']) # sac.SACComponentAgent(env)
agents['rolltofaceup'].build_inference_networks()
agents['rolltofaceup'].lf.policy_net.load_named_checkpoint('LFRollToFaceUp251102a_60000')
agents['rolltofaceup'].rf.policy_net.load_named_checkpoint('RFRollToFaceUp251102a_60000')
agents['rolltofaceup'].segs.policy_net.load_named_checkpoint('SegsRollToFaceUp251102a_60000')

agents['rolltofacedown'] = sac.SACComponentsAgent(machines['rolltofacedown']) # sac.SACComponentAgent(env)
agents['rolltofacedown'].build_inference_networks()
agents['rolltofacedown'].lf.policy_net.load_named_checkpoint('LFRollToFaceDown251102a_60000')
agents['rolltofacedown'].rf.policy_net.load_named_checkpoint('RFRollToFaceDown251102a_60000')
agents['rolltofacedown'].segs.policy_net.load_named_checkpoint('SegsRollToFaceDown251102a_60000')


agents['fdfw'] = sac.SACComponentsAgent(machines['fdfw']) # sac.SACComponentAgent(env)
agents['fdfw'].build_inference_networks()
agents['fdfw'].lf.policy_net.load_named_checkpoint('LFDemiQuadFDFW251106a_120000_120000')
agents['fdfw'].rf.policy_net.load_named_checkpoint('RFDemiQuadFDFW251106a_120000_120000')
# agents['fdfw'].segs.policy_net.load_named_checkpoint('SegsRollToFaceUp251106a_60000')


agents['fdbw'] = sac.SACComponentsAgent(machines['fdbw']) # sac.SACComponentAgent(env)
agents['fdbw'].build_inference_networks()
agents['fdbw'].lf.policy_net.load_named_checkpoint('LFDemiQuadFDBW251107a_120000_120000')
agents['fdbw'].rf.policy_net.load_named_checkpoint('RFDemiQuadFDBW251107a_120000_120000')
# agents['fdbw'].segs.policy_net.load_named_checkpoint('SegsRollToFaceDown251106a_60000')


agents['fdlt'] = sac.SACComponentsAgent(machines['fdlt']) # sac.SACComponentAgent(env)
agents['fdlt'].build_inference_networks()
agents['fdlt'].lf.policy_net.load_named_checkpoint('LFDemiQuadFDLT251106a_30000')
agents['fdlt'].rf.policy_net.load_named_checkpoint('RFDemiQuadFDLT251106a_30000')
# agents['fdlt'].segs.policy_net.load_named_checkpoint('SegsRollToFaceUp251106a_60000')


agents['fdrt'] = sac.SACComponentsAgent(machines['fdrt']) # sac.SACComponentAgent(env)
agents['fdrt'].build_inference_networks()
agents['fdrt'].lf.policy_net.load_named_checkpoint('LFDemiQuadFDRT251107a_30000')
agents['fdrt'].rf.policy_net.load_named_checkpoint('RFDemiQuadFDRT251107a_30000')
# agents['fdrt'].segs.policy_net.load_named_checkpoint('SegsRollToFaceDown251106a_60000')












current_skill = 'rolltofaceup' # rolltofaceup rolltofacedown





# {"states":[0.0,0.0,0.0,0.0]}
# {"actions":[0.0,0.0]}
topic_message_template = {}
topic_message_template['topic'] = ''
topic_message_template['messagestring'] = ''

skill_service_template = {}
skill_service_template['command'] = ''
skill_service_template['param1'] = ''
skill_service_template['param2'] = ''

state_template = {}
state_template['states'] = []

action_template = {}
action_template['actions'] = []



service_request_topic = 'demiquad/skill/servicerequest'
service_respond_topic = 'demiquad/skill/servicerespond'
action_topic = 'demiquad/skill/action'
state_topic = 'demiquad/skill/state'




broker = '127.0.0.1' # 192.168.137.33 # 192.168.2.18 # 127.0.0.1
port = 1883




# Generate a Client ID with the publish prefix.
client_id = f'publish-{random.randint(0, 1000)}'
# username = 'emqx'
# password = 'public'

def is_number(s):
    try:
        float(s)
        return True
    except ValueError:
        return False
        
        
def connect_mqtt():
    def on_connect(client, userdata, flags, rc):
        if rc == 0:
            print("Connected to MQTT Broker!")
        else:
            print("Failed to connect, return code %d\n", rc)

    client = mqtt_client.Client(client_id)
    # client.username_pw_set(username, password)
    client.on_connect = on_connect
    client.connect(broker, port)
    return client


def publish(client):
    msg_count = 1
    while True:
        time.sleep(1)
        msg = f"messages: {msg_count}"
        result = client.publish(topic, msg)
        # result: [0, 1]
        status = result[0]
        if status == 0:
            print(f"Send `{msg}` to topic `{topic}`")
        else:
            print(f"Failed to send message to topic {topic}")
        msg_count += 1
        if msg_count > 15:
            break

def publish_action(client, message_in):
    

    state = message_in['states']
    # print(state)
    print(current_skill)
    
    # test the state againts the lightweight env to see if 
    # the skill is completed
    # if not complete, use state to get next action and publish action on action_topic
    # if complete, publish skill completed command on service_respond_topic
    
    is_complete = machines[current_skill].inference_success(state)
    
    # run a step in the model
    actions = inference_state(state)
    

    # I don't run this here, I run it on the sending node
    # state, reward, done, info, _ = env.step( action_segs.tolist() ) 
       
    status = 0
    if(is_complete):
        skill_service_template['command'] = 'skillcomplete'
        skill_service_template['param1'] = current_skill
        skill_service_template['param2'] = ''
        msg_str =  json.dumps(skill_service_template)
        result = client.publish(service_respond_topic, msg_str)
        status = result[0] # result: [0, 1]
    else:
        action_template['actions'] = actions
        msg_str =  json.dumps(action_template)
        result = client.publish(action_topic, msg_str)
        status = result[0] # result: [0, 1]

    
    
    if status == 0:
        x = 0 # print(f"Send `{msg_str}` to topic `{action_topic}`")
    else:
        print(f"Failed to send message to topic {action_topic}")


def inference_state(state):
    
    if(skillrig_descs[current_skill].json["actionMode"] == 1):
        return inference_state_ZXYEZXYE(state)
    elif(skillrig_descs[current_skill].json["actionMode"] == 2):
        return inference_state_ZXYEZXYESEGS(state)
    else:
        return [0]
    
    
    
def inference_state_ZXYEZXYE(state):
    action_lf = agents[current_skill].lf.policy_net.get_action(state) # .detach()
    action_rf = agents[current_skill].rf.policy_net.get_action(state) # .detach()
    actions = action_lf.tolist() + action_rf.tolist()
    return actions


def inference_state_ZXYEZXYESEGS(state):
    action_lf = agents[current_skill].lf.policy_net.get_action(state) # .detach()
    action_rf = agents[current_skill].rf.policy_net.get_action(state) # .detach()
    action_segs = agents[current_skill].segs.policy_net.get_action(state) # .detach()
    actions = action_lf.tolist() + action_rf.tolist() + action_segs.tolist()
    return actions
    
    
    
def service(client, message_in):

    # print("current_skill request: ", message_in)
    
    # respond to skill requests
    # load the corrent skilldescriptor, and load the models
    # skill_service_template['command'] = ''
    # skill_service_template['param1'] = ''
    # skill_service_template['param2'] = ''
    
    global current_skill
    if(message_in['command'] == 'loadskillrig'):
        current_skill = message_in['param1']
        print("current_skill request: ", current_skill)
    
    
   

def subscribe(client: mqtt_client):
    def on_message(client, userdata, msg):
        # print(f"Received `{msg.payload.decode()}` from `{msg.topic}` topic")
        m = json.loads(msg.payload.decode())
        if(msg.topic == state_topic):
            if( m.keys() == state_template.keys() ):
                publish_action(client, m)
        elif(msg.topic == service_request_topic):
            if( m.keys() == skill_service_template.keys() ):
                service(client, m)
              
              
    # client.subscribe([(state_topic, 0), (service_request_topic, 0)])
    client.subscribe(state_topic)
    client.subscribe(service_request_topic)
    
    
    client.on_message = on_message
    
    
def run():
    client = connect_mqtt()
    subscribe(client)
    client.loop_forever()


if __name__ == '__main__':
    run()
    
    
    
   
    
