import Vue from 'vue'
import Vuex from 'vuex'
import {backendUrl,confAppBarTitle} from '@/yuki.config.js'
Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    jwt:null,
    firstopen:true,
    loginUsr:null,
    afterRegister:false,
    enabledPages:null,
    backendUrl,
    confAppBarTitle
  },
  mutations: {
    setjwt (state, jwt) {
      if(jwt!==null&&jwt!=='') {
        state.jwt='Bearer '+jwt;
        localStorage.setItem("ycmsjwt", state.jwt);
      }else{
         state.jwt=null;
         localStorage.removeItem("ycmsjwt")
      }
      
    },
    setLoginUsr(state,loginUsr){
      state.loginUsr=loginUsr
    },
    setEnabledPages(state,enabledPages){
      state.enabledPages=enabledPages
    },
    afterRegister(state){
      state.afterRegister=true;
    },
    arHintShowed(state){
      state.afterRegister=false;
    },
    logout(state){
      localStorage.removeItem("ycmsjwt")
      state.jwt=null;
      state.loginUsr=null;
      state.enabledPages=null;
    },
    loadFromLocalStorage(state){
      state.jwt=localStorage.getItem("ycmsjwt");
      state.firstopen=false;
    },
  },
  actions: {

  },
  modules: {
  }
})
