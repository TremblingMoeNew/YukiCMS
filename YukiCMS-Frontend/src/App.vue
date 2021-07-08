<template>
  <v-app>
    <v-app-bar app color="primary" dark clipped-left>
      <div class="d-flex align-center">
        <v-app-bar-nav-icon @click="drawer=!drawer" class="d-lg-none" tile v-if="loginUsr!==null">
        </v-app-bar-nav-icon>
        
      </div>
      <v-toolbar-title>{{confAppBarTitle}}</v-toolbar-title>
      <v-spacer></v-spacer>
      
      <v-menu offset-y v-if="loginUsr!==null">
        <template v-slot:activator="{ on, attrs }">
          <v-btn v-bind="attrs" v-on="on" icon>
            <v-icon >mdi-dots-vertical</v-icon>
          </v-btn>
        </template>
        <v-list>
          <v-list-item two-line>
            <v-list-item-content>
              <v-list-item-title>{{loginUsr.name}}</v-list-item-title>
              <v-list-item-subtitle>{{loginUsr.email}}</v-list-item-subtitle>
            </v-list-item-content>
          </v-list-item>
          <v-divider></v-divider>
          <v-list-item link @click="$router.push({name:'User-Info'})">
            <v-list-item-title>个人信息</v-list-item-title>
          </v-list-item>
          <v-list-item link @click="logout">
            <v-list-item-title>退出登录</v-list-item-title>
          </v-list-item>
          
        </v-list>
      </v-menu>
    </v-app-bar>
      <v-navigation-drawer v-model="drawer" app clipped  v-if="loginUsr!==null && enabledPages!==null">
        <v-list>
          <v-list-item-group  v-model="listItemGroupCurItem" mandatory>
            <v-list-item link @click="$router.push({name:'Home'})">
              <v-list-item-title>首页</v-list-item-title>
            </v-list-item>
            <template  v-if="enabledPages.withdrawed!==true">
              <v-list-group no-action>
                <template v-slot:activator >
                    <v-list-item-title >会场</v-list-item-title>
                </template>
                <v-list-item link @click="$router.push({name:'Committees-Management'})" v-if="enabledPages.enableCommitteeManagement===true">
                  <v-list-item-title>管理</v-list-item-title>
                </v-list-item>
                <v-list-item link @click="$router.push({name:'Committee-Apply'})">
                  <v-list-item-title>申请</v-list-item-title>
                </v-list-item>
                <template v-for="(com,idx) of visibleCommittees">
                  <v-list-group sub-group no-action  :key="idx"  @click="$router.push({name:'Committee-Main',params:{cid:com.cid}})">
                    <template v-slot:activator>
                        <v-list-item-title>{{com.name}}</v-list-item-title>
                    </template>
                    <v-list-item link @click="$router.push({name:'Committee-Reviews-Main',params:{cid:com.cid}})">
                      <v-list-item-title>学测</v-list-item-title>
                    </v-list-item>
                    <v-list-item link @click="$router.push({name:'Committee-Seats-Management',params:{cid:com.cid}})" v-if="com.enableSeatManagement===true">
                      <v-list-item-title>席位</v-list-item-title>
                    </v-list-item>
                  </v-list-group>
                </template>
              </v-list-group>
              <v-list-item link @click="$router.push({name:'Files-Main'})" >
                <v-list-item-title>文件</v-list-item-title>
              </v-list-item>
              <v-list-group no-action @click="$router.push({name:'Accommodation-Main'})" v-if="false && enabledPages.enableAccManagement===true">
                <template v-slot:activator >
                    <v-list-item-title>住宿</v-list-item-title>
                </template>
                <v-list-item link >
                  <v-list-item-title>住宿分配</v-list-item-title>
                </v-list-item>
              </v-list-group>
              <v-list-item v-else @click="$router.push({name:'Accommodation-Main'})">
                <v-list-item-title>住宿</v-list-item-title>
              </v-list-item>
            </template>
            <v-list-group no-action @click="$router.push({name:'Payments-Main'})" v-if="enabledPages.enablePaymentManagement===true && enabledPages.withdrawed!==true">
              <template v-slot:activator >
                  <v-list-item-title>账单</v-list-item-title>
              </template>
              <v-list-item link @click="$router.push({name:'Payments-Management'})" >
                <v-list-item-title>账单管理</v-list-item-title>
              </v-list-item>
            </v-list-group>
            <v-list-item link v-else @click="$router.push({name:'Payments-Main'})">
              <v-list-item-title>账单</v-list-item-title>
            </v-list-item>
            <template  v-if="enabledPages.withdrawed!==true">
              <v-list-group no-action @click="$router.push({name:'Permissions-Main'})" v-if="enabledPages.enablePermissionGroupManagement===true">
                <template v-slot:activator >
                    <v-list-item-title>权限</v-list-item-title>
                </template>
                <v-list-item link @click="$router.push({name:'Permission-Groups-Management'})" >
                  <v-list-item-title>权限组管理</v-list-item-title>
                </v-list-item>
              </v-list-group>
              <v-list-item link @click="$router.push({name:'Permissions-Main'})" v-else>
                <v-list-item-title>权限</v-list-item-title>
              </v-list-item>
              <v-list-group no-action v-if="enabledPages.enableGlobalSettings===true || enabledPages.enableUserManagement===true">
                <template v-slot:activator >
                    <v-list-item-title>高级</v-list-item-title>
                </template>
                <v-list-item link @click="$router.push({name:'Global-Settings'})" v-if="enabledPages.enableGlobalSettings===true">
                  <v-list-item-title>全局设置</v-list-item-title>
                </v-list-item>
                <v-list-item link @click="$router.push({name:'Users-Management'})" v-if="enabledPages.enableUserManagement===true">
                  <v-list-item-title>用户管理</v-list-item-title>
                </v-list-item>
              </v-list-group>
            </template>
          </v-list-item-group>
        </v-list>
      </v-navigation-drawer>
    <v-main>
      <div fluid class="fill-height fill-width">
        <router-view @reload="reload()"/>
      </div>
    </v-main>
  </v-app>
</template>

<script>
//import HelloWorld from './components/HelloWorld';
//import YLogin from './views/Login'
//import Register from './views/Register'
//import YCommitteeApply from './views/Committees/Committee-Apply'
//import YCommitteeMain from './views/Committees/Committee-Main'
//import YCommitteeSeatsManagement from './views/Committees/Committee-Seats-Management'
//import YCommitteeManagement from './views/Committees/Committee-Management'
//import YCommitteeReviewsMain from './views/Committees/Committee-Reviews-Main'
//import YCommitteeReviewMain from './views/Committees/Committee-Review-Main'
//import YUserInfoEdit from './views/User-Info-Edit'
//import YPaymentsManagement from './views/payments/Payments-Management'
//import YAccommodationMain from './views/Accommodation/Accommodation-Main'
//import YPermissionGroupManagement from './views/permissions/Permission-Group-Management'
//import YUserManagement from './views/Advanced/User-Management'
import mixin from './utils/mixins'
export default {
  name: 'App',
  mixins:[mixin],
  data: () => ({
    drawer:null,
    active:true,
    visibleCommittees:[],
    listItemGroupCurItem:0,
  }),
  computed:{
    jwt:function(){
      return this.$store.state.jwt;
    },
    confAppBarTitle:function(){
      return this.$store.state.confAppBarTitle
    }
  },
  watch:{
    jwt:function(njwt){
      if(njwt!==null)this.fetchLoginUsr();
    },
    loginUsr:function(nusr){
      if(nusr!==null){
        this.reload();
      }
      else{
        this.$router.push({name:'Login'})
      }
    },
    enabledPages:function(npages,opages){
      if(npages!==null&&opages===null){
        this.$router.push({ path: this.$route.query.redirect || '/' })
      }
    }
  },
  methods:{
    fetchLoginUsr:function(){
      var vm=this;
      this.fetchGet('/api/user/name')
      .then(function(res){
        if(res.status===200){
          res.json().then(function(json){
            vm.$store.commit('setLoginUsr',json)
          })
        }
      })
    },
    fetchVisibleCommittees:function(){
      var vm=this;
      this.fetchGet('/api/committee/view')
      .then(function(res){
        if(res.status===200){
          res.json().then(function(json){
            vm.visibleCommittees=json;
          })
        }
      })
    },
    fetchEnabledPages:function(){
      var vm=this;
      this.fetchGet('/api/permission/vertify/home')
      .then(function(res){
          res.json().then(function(json){
            vm.$store.commit('setEnabledPages',json)
          })
      })
    },
    reload:function(){
      this.fetchVisibleCommittees();
      this.fetchEnabledPages();
    },
    logout:function(){
      this.$store.commit('logout')
    }
  },
};
</script>
