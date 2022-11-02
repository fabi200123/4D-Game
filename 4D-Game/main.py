# Modules
import pyrebase
import streamlit as st
from datetime import datetime

# Config Key
firebaseConfig = {
    'apiKey': "AIzaSyAvAR5POEPiVKcq1BD_Pk_xEhbWWwwlWNg",
    'authDomain': "d-game-b2eb3.firebaseapp.com",
    'projectId': "d-game-b2eb3",
    'databaseURL': "https://d-game-b2eb3-default-rtdb.europe-west1.firebasedatabase.app/",
    'storageBucket': "d-game-b2eb3.appspot.com",
    'messagingSenderId': "444806286330",
    'appId': "1:444806286330:web:ec374ca229efc9579a7243",
    'measurementId': "G-79YBNF0GLN"
}

# Firebase Auth
firebase = pyrebase.initialize_app(firebaseConfig)
auth = firebase.auth()

# Database
db = firebase.database()
storage = firebase.storage()

st.sidebar.title("4D Simulator")

# Authentication
choice = st.sidebar.selectbox('login/Signup', ['Login', 'Sign Up'])

email = st.sidebar.text_input('Please enter your email address')
password = st.sidebar.text_input('Please enter your password', type='password')

if choice == 'Sign Up':
    username = st.sidebar.text_input("Please input your username", value='')
    submit = st.sidebar.button('Create my account')

    if submit:
        user = auth.create_user_with_email_and_password(email, password)
        st.success("Account created succesfully")
        st.balloons()

        # Sign in
        user = auth.sign_in_with_email_and_password(email, password)
        db.child(user['localId']).child("Username").set(username)
        db.child(user['localId']).child("ID").set(user['localId'])
        st.title('Welcome ' + username)
        st.info('Login via login drop down selection')
if choice == 'Login':
    login = st.sidebar.button('Login')
    if login:
        user = auth.sign_in_with_email_and_password(email, password)
        st.write('<style>div.row-widget.stRadio > div{flex-direction:row;}</style>', unsafe_allow_html=True)
        bio = st.radio('Jump to', ['Home', 'Game', 'Settings'])

        if bio == 'Home':
            username = db.child(user['localId']).child("Username").get().val()
            bg = st.info("Here is home, " + username)
        elif bio == 'Game':
            bg = st.info("Here should be the game!")
        else:
            bg = st.info("We wait to add Settings!")
