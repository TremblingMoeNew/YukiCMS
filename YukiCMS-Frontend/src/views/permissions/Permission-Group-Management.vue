<template>
    <div>
        <div class="text-h5 mx-6 mt-6">权限组管理</div>
        <template v-if="enabledPages.enablePermissionGroupManagement!==true" >
            <v-divider class="mx-9 mt-6"/>
            <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto">
                您没有查看的权限
            </div>
        </template>
        <template v-else>
            <v-container>
                <v-row>
                    <v-col order="3" order-md="1" cols="12">
                        <div class="d-flex align-center">
                            <v-tabs v-model="tab" right center-active show-arrows>
                                <v-tab>权限列表</v-tab>
                                <v-btn text class="grey--text darken-2 align-self-center" v-if="permission.grantRevokePermissions===true" @click.stop="openGrantPermissionDialog()">
                                    赋权
                                </v-btn>
                                <v-tab>成员列表</v-tab>
                                <v-btn text class="grey--text darken-2 align-self-center" v-if="permission.cusGroupsTransferMember===true && permission_response.isFreeGroup===true" @click.stop="openAddMemberDialog()">
                                    添加成员
                                </v-btn>
                                <v-btn text class="grey--text darken-2 align-self-center" v-if="permission.createCusGroups===true && permission_response.isFreeGroup===true" @click.stop="openRemovePermissionGroupDialog()">
                                    删除权限组
                                </v-btn>
                            </v-tabs>
                        </div>
                        <v-divider/>
                    </v-col>
                    <v-col order="2" order-md="4" md="4" cols="12">
                        <v-card class="mx-2 my-2">
                            <v-card-title>权限组 {{permission_response.name}}</v-card-title>
                            <v-card-text>
                                <div class="mx-12 my-0">
                                    <p v-if="permission_response.isFreeGroup===true" class="green--text darken-1">
                                        自由权限组
                                    </p>
                                    <p v-else-if="permission_response.cid!==0">
                                        隶属于会场： <span class="deep-orange--text accent-1">{{permission_response.cname}}</span>
                                    </p>
                                    <p>权限数量： {{permission_response.permissionList.length}}</p>
                                    <p>成员数量： {{permission_response.members.length}}</p>
                                </div>
                            </v-card-text>
                        </v-card>
                    </v-col>
                    <v-col order="1" order-md="3" md="auto" cols="12">
                        <v-divider class="d-none d-lg-flex" vertical/>
                        <v-divider class="d-flex d-lg-none my-5"/> 
                    </v-col>
                    <v-col order="4" order-md="2">
                        <v-tabs-items v-model="tab">
                            <v-tab-item>
                                <p class="text-h6">权限</p>
                                <v-data-table :headers="pr_headers" :items.sync="permission_response.permissionList" @dblclick:row="openRevokePermissionDialog"/>
                            </v-tab-item>
                            <v-tab-item>
                                <p class="text-h6">成员</p>
                                <v-data-table :headers="ur_headers" :items.sync="permission_response.members" @dblclick:row="openRemoveMemberDialog"/>
                            </v-tab-item>
                        </v-tabs-items>
                    </v-col>
                </v-row>
            </v-container>
            <v-dialog v-model="notedialog.active" width="200">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text>
                        <p v-if="notedialog.success===true" class="d-flex justify-center">操作成功！</p>
                        <p v-else class="d-flex justify-center">操作失败！</p>
                    </v-card-text>
                </v-card>
            </v-dialog>
            <v-dialog v-model="revokePermissionDialog" width="500" v-if="activePer!==null">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">您确定要移除权限
                            <span  class="green--text darken-1"> {{activePer.ptype_str}} </span>吗？
                        </p>
                        <p class="mx-10">
                            (权限作用域： 
                            <span  class="primary--text">{{activePer.scope}}</span>
                            — 
                            <span  class="deep-orange--text accent-2">{{activePer.pObjectName}}</span>
                            )
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="revokePermissionDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" @click="submitRevokePermission();">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="removeMemberDialog" width="400" v-if="activeUsr!==null">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">您确定要移除用户
                            <span  class="green--text darken-1"> {{activeUsr.name}} </span>吗？
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="removeMemberDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" @click="submitRemoveMember();">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="removePermissionGroupDialog" width="400">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">您确定要移除权限组
                            <span  class="green--text darken-1"> {{permission_response.name}} </span>吗？
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="removePermissionGroupDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" @click="submitRemoveGroup();">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="grantPermissionDialog" width="400">
                <v-card width="400">
                    <v-toolbar dense flat color="blue lighten-4">
                        <v-toolbar-title>赋权</v-toolbar-title>
                        <v-spacer></v-spacer>
                    </v-toolbar>
                    <v-card-text>
                        <div class="mx-3 mt-6">
                            <v-select :items="visiblePermissionList" item-text="name" item-value="value" label="权限" v-model="newPer.ptype" @change="adjustNewPerPObjectId()"/>
                            <v-select :items="committee_list" item-text="name" item-value="cid" label="作用域（会场）" v-model="newPer.pObjectId" v-if="newPer.ptype!==null && Permisson_List[newPer.ptype].committee===true" />
                            <v-select :items="filegroup_list" item-text="name" item-value="fgid" label="作用域（文件组）" v-model="newPer.pObjectId" v-if="newPer.ptype!==null && Permisson_List[newPer.ptype].filegroup===true"/>
                            <div v-if="newPer.ptype!==null && Permisson_List[newPer.ptype].prerequisities">
                                <p>该权限的正常生效依赖于以下权限：</p>
                                <p class="d-flex justify-center deep-orange--text accent-2" v-for="(type,idx) of Permisson_List[newPer.ptype].prerequisities" :key="idx">
                                    {{Permisson_List[type].name}}
                                </p>
                            </div>
                        </div>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn class="mx-2" color="blue lighten-4" :disabled="newPer.pObjectId===null" @click.stop="submitGrantPermission()">赋权</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="addMemberDialog" width="400">
                <v-card width="400">
                    <v-toolbar dense flat color="blue lighten-4">
                        <v-toolbar-title>添加成员</v-toolbar-title>
                        <v-spacer></v-spacer>
                    </v-toolbar>
                    <v-card-text>
                        <div class="mx-3 mt-6">
                            <v-select :items="usr_list" item-text="name" item-value="uid" label="代表" v-model="newuid"/>
                        </div>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn class="mx-2" color="blue lighten-4" :disabled="newuid===null" @click.stop="submitAddMember()">添加</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
        </template>
    </div>
</template>
<script>
import {Permisssion_Type,Permisson_List} from "@/utils/permission"
import mixin from '@/utils/mixins'
export default {
    name:"Permission-Group-Management",
    mixins:[mixin],
    props:['pgid'],
    data:()=>({
        tab:null,
        notedialog:{
            active:null,
            success:true,
        },
        revokePermissionDialog:null,
        removeMemberDialog:null,
        removePermissionGroupDialog:null,
        grantPermissionDialog:null,
        addMemberDialog:null,
        activePer:null,
        activeUsr:null,
        newPer:{
            ptype:null,
            pObjectId:null,
        },
        newuid:null,
        permission_response:[],
        permission:{
            createCusGroups:false,
            getGrps:false,
            grantRevokePermissions:false,
            cusGroupsTransferMember:false,
        },
        pr_headers:[
            {text:'权限',value:'ptype_str'},
            {text:'类型',value:'scope'},
            {text:'作用域',value:'pObjectName'},
        ],
        ur_headers:[
            {text:'姓名',value:'name'},
            {text:'邮箱',value:'email'},
            {text:'性别',value:'sex'},
            {text:'电话号码',value:'phoneNumber'},
            {text:'QQ号',value:'qqNumber'},
            {text:'微信号',value:'wechatNumber'},
        ],
        committee_list:[
        ],
        filegroup_list:[
        ],
        usr_list:[
        ],
        Permisssion_Type,
        Permisson_List
    }),
    computed:{
        visiblePermissionList:function(){
            return this.Permisson_List.slice(1);
        },
    },
    watch:{
        permission_response:async function(){
            this.getPermission();
            this.getUsrList();
            this.getCommitteeList();
            this.getFGList();
        }
    },
    methods:{
        getPermissionResponse:function(){
            var vm=this;
            this.permission_response=[]
            this.fetchGet('/api/permission/group/'+vm.pgid)
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.initPRPermissionList(json);
                        vm.permission_response=json;
                    })
                }
            })
        },
        getPermission:async function() {
            var vm=this;
            this.fetchGet('/api/permission/vertify/permissiongrp')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.permission=json;
                    })
                }
            })
        },
        getUsrList:async function(){
            var vm=this;
            this.fetchGet('/api/permission/group/'+vm.pgid+'/potential')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.usr_list=json;
                    })
                }
            })
        },
        getCommitteeList:async function(){
            var vm=this;
            this.fetchGet('/api/committee')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.committee_list=json;
                    })
                }
            })
        },
        getFGList:async function(){
            var vm=this;
            this.fetchGet('/api/filegroup/list')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.filegroup_list=json;
                    })
                }
            })
        },
        initPRPermissionList:function(pr){
            for(var i=0;i<pr.permissionList.length;i++){
                var pitem=pr.permissionList[i];
                if(pitem.ptype>=this.Permisson_List.length){
                    pr.permissionList.splice(i,1);
                    i--;
                    continue;
                }
                var pt=this.Permisson_List[pitem.ptype];
                pitem.ptype_str=pt.name;
                if(pt.global===true)
                {
                    pitem.scope='全局权限';
                    pitem.pObjectName='全局';
                }
                else if (pt.committee===true)
                    pitem.scope='会场权限';
                else if (pt.filegroup===true)
                    pitem.scope='文件组权限';
            }
        },
        openRevokePermissionDialog:function(_,event){
            if(this.permission.grantRevokePermissions!==true)return;
            this.activePer=event.item;
            if(this.activePer.isFreeGroup!==true){
                var pt=this.Permisson_List[this.activePer.ptype];
                if(pt.lock){
                    if(pt.global
                    ||(pt.committee && this.activePer.pObjectId===this.permission_response.cid)
                    ||(pt.filegroup && 
                            (this.activePer.pObjectId===0 || this.activePer.pObjectId===this.permission_response.fgrpid)
                        )
                    )
                    return;
                }
            }
            this.revokePermissionDialog=true;
        },
        openRemoveMemberDialog:function(_,event){
            if(this.permission.cusGroupsTransferMember!==true)return;
            if(this.permission_response.isFreeGroup!==true)return;
            this.activeUsr=event.item;
            this.removeMemberDialog=true;
        },
        submitRevokePermission:function(){
            var vm=this;
            vm.fetchDelete('/api/permission/group/'+vm.pgid+'/'+vm.activePer.ptype+'/'+vm.activePer.pObjectId)
                .then(function(res){
                if(res.status===204){
                    vm.permission_response.permissionList.splice(vm.permission_response.permissionList.indexOf(vm.activePer),1);
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.revokePermissionDialog=false;
        },
        submitRemoveMember:function(){
            var vm=this;
            vm.fetchDelete('/api/permission/group/'+vm.pgid+'/u'+vm.activeUsr.uid)
                .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.permission_response.members.splice(vm.permission_response.members.indexOf(vm.activeUsr),1);
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            
            this.removeMemberDialog=false;
            this.notedialog.active=true;
        },
        openRemovePermissionGroupDialog:function(){
            this.removePermissionGroupDialog=true;
        },
        submitRemoveGroup:function(){
            var vm=this;
            vm.fetchDelete('/api/permission/group/'+vm.pgid)
                .then(function(res){
                if(res.status===204){
                    vm.$router.push({name:'Permission-Groups-Management'})
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.removePermissionGroupDialog=false;
        },
        openGrantPermissionDialog:function(){
            this.newPer={ptype:null,pObjectId:null}
            this.grantPermissionDialog=true;
        },
        adjustNewPerPObjectId:function(){
            if(this.newPer.ptype!==null && this.Permisson_List[this.newPer.ptype].global===true)
                this.newPer.pObjectId=0;
            else
                this.newPer.pObjectId=null;
        },
        submitGrantPermission:function(){
            var vm=this;
            vm.fetchPostJson('/api/permission/group/'+vm.pgid,vm.newPer)
                .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.getPermissionResponse();
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.grantPermissionDialog=false;
        },
        openAddMemberDialog:function(){
            this.newuid=null;
            this.addMemberDialog=true;
        },
        submitAddMember:function(){
            var vm=this;
            vm.fetchPostJson('/api/permission/group/'+vm.pgid+'/u'+vm.newuid,{})
                .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.getPermissionResponse();
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.addMemberDialog=false;
        },
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getPermissionResponse())
    },
    beforeRouteUpdate (to, from, next) {
        this.getPermissionResponse();
        next()
    },

}
</script>