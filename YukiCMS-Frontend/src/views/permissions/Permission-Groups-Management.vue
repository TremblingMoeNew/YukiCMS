<template>
    <div>
        <div class="text-h5 mx-6 mt-6">权限组管理</div>
        <template  v-if="enabledPages.enablePermissionGroupManagement!==true" >
            <v-divider class="mx-9 mt-6"/>
            <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto">
                您没有查看的权限
            </div>
        </template>
        <template v-else>
            <template v-if="permission.createCusGroups===true">
                <div class="d-flex align-center mx-9">
                    <v-tabs>
                        <v-spacer />
                        <v-btn text class="grey--text darken-2 align-self-center" @click.stop="openCreateFGDialog">创建自由组</v-btn>
                    </v-tabs>
                </div>
                <v-dialog v-model="createFGDialog" width="400">
                    <v-card width="400">
                        <v-toolbar dense flat color="blue lighten-4">
                            <v-toolbar-title>创建权限组</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>
                            <v-text-field label="名称" v-model="fgname" @keypress.enter="createFG"/>
                        </v-card-text>
                        <v-card-actions>
                            <v-spacer />
                            <v-btn class="mx-2" color="blue lighten-4" @click.stop="createFG">搜索</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>
            </template>
            <template v-else>
                <div class="mt-6"/>
            </template>
            <v-divider class="mx-9"/>
            <v-container v-if="groups!==null">
                <v-row>
                    <v-col>
                        <v-data-table :headers="header" :items="groups" @dblclick:row="openGroup"/>
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
        </template>
    </div>
</template>
<script>
import {Permisssion_Type,Permisson_List} from "@/utils/permission"
import mixin from '@/utils/mixins'
export default {
    name:"Permission-Groups-Management",
    mixins:[mixin],
    data:()=>({
        createFGDialog:null,
        fgname:null,
        notedialog:{
            active:null,
            success:true,
        },
        header:[
            {text:'编号',value:'pgid'},
            {text:'名称',value:'name'},
            {text:'权限数量',value:'permissionList.length'},
            {text:'成员数量',value:'members.length'},
            {text:'自由组',value:'isFreeGroup_hint'},
        ],
        groups:null,
        permission:{
            getGrps:false,
            createCusGroups:false,
        },
        Permisssion_Type,
        Permisson_List
    }),
    methods:{
        initGroups:function(grps){
            for(var i=0;i<grps.length;i++)
                grps[i].isFreeGroup_hint=grps[i].isFreeGroup?"是":"否"
        },
        openCreateFGDialog:function(){
            this.fgname=null;
            this.createFGDialog=true;
        },
        getPermission:function() {
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
        createFG:function(){
            var vm=this;
            vm.fetchPostJson('/api/permission/group',vm.fgname)
                .then(function(res){
                if(res.status===201){
                    res.json().then(function(json){
                        json.isFreeGroup_hint=json.isFreeGroup?"是":"否"
                        vm.groups.push(json)
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                    })
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.createFGDialog=false;
        },
        getGroups:function(){
            var vm=this;
            this.fetchGet('/api/permission/group')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.initGroups(json)
                        vm.groups=json;
                        vm.getPermission();
                    })
                }
            })
        },
        openGroup:function(_,{item}){
            this.$router.push({name:'Permission-Group-Management',params:{pgid:item.pgid}})
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getGroups())
    },
    beforeRouteUpdate (to, from, next) {
        this.getGroups();
        next()
    },
}
</script>