<template>
    <div>
        <v-data-table :headers="tmpl_header" :items="templates" @click:row="expand" class="mx-12 my-5" calculate-widths :loading="loading" loading-text="加载中……" :expanded.sync="expanded" show-expand single-expand  item-key="ttid">
            <template v-slot:expanded-item="{ headers, item }">
                <td :colspan="headers.length">
                    <v-card flat class="my-1">
                        <v-card-text>       
                            <p v-for="(question,j) of item.questions" :key="j" class="mx-3 my-1">{{j+1}}： {{question}}</p>
                        </v-card-text>
                    </v-card>
                </td>
            </template>
            <template v-slot:item.actions="{ item }">
                <v-btn text color="primary" :disabled="permission.pushReviews!==true" v-if="item.autopushed===true" @click.stop="openUnpushDialog(item)">停止推送</v-btn>
                <v-btn text color="primary" :disabled="permission.pushReviews!==true" v-else @click.stop="openPushDialog(item)">推送</v-btn>
                <v-btn text color="deep-orange accent-2" :disabled="permission.createReviewTemplate!==true || item.autopushed===true" @click.stop="openDeleteDialog(item)">删除</v-btn>
            </template>
            <template v-slot:item.data-table-expand />
        </v-data-table>
        <v-dialog v-model="notedialog.active" width="200">
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text>
                    <p v-if="notedialog.success===true" class="d-flex justify-center">操作成功！</p>
                    <p v-else class="d-flex justify-center">操作失败！</p>
                </v-card-text>
            </v-card>
        </v-dialog>
        <v-dialog v-model="pushTmplDialog" width="400">
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text style="text-align: center;">
                    <p class="mt-6">您确定要向会场
                        <span  class="green--text darken-1"> {{com.name}} </span>
                        推送模板
                        <span  class="deep-orange--text accent-2"> {{activeTmpl.name}} </span>
                        吗？
                    </p>
                </v-card-text>
                <v-card-actions>
                    <v-spacer />
                    <v-btn text @click="pushTmplDialog=false;">取消</v-btn>
                    <v-btn text color="deep-orange accent-2"  @click="submitPush();">确定</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog v-model="unpushTmplDialog" width="400">
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text>
                    <div class=" d-flex justify-center">
                    <p class="mt-6">您确定停止向会场
                        <span  class="green--text darken-1"> {{com.name}} </span>
                        推送模板
                        <span  class="deep-orange--text accent-2"> {{activeTmpl.name}} </span>
                        吗？
                    </p>
                    </div>
                </v-card-text>
                <v-card-actions>
                    <v-spacer />
                    <v-btn text @click="unpushTmplDialog=false;">取消</v-btn>
                    <v-btn text color="deep-orange accent-2"  @click="submitUnpush();">确定</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog v-model="deleteTmplDialog" width="400">
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text>
                    <div class=" d-flex justify-center">
                    <p class="mt-6">您确定要删除模板
                        <span  class="deep-orange--text accent-2"> {{activeTmpl.name}} </span>
                        吗？
                    </p>
                    </div>
                </v-card-text>
                <v-card-actions>
                    <v-spacer />
                    <v-btn text @click="deleteTmplDialog=false;">取消</v-btn>
                    <v-btn text color="deep-orange accent-2" :disabled="permission.createReviewTemplate!==true" @click="submitDelete();">确定</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>
<script>
import mixin from '@/utils/mixins'
export default {
    name:"Committee-Review-Tmpls-Management",
    mixins:[mixin],
    model:{
        prop:'templates',
        event:'change',
    },
    props:{
        com:Object,
        templates:Array,
        permission:Object
    },
    data:()=>({
        expanded:null,
        deleteTmplDialog:null,
        pushTmplDialog:null,
        unpushTmplDialog:null,
        activeTmpl:{},
        notedialog:{
            active:null,
            success:true,
        },
        tmpl_header:[
            {text:'模板编号',value:'ttid'},
            {text:'模板名',value:'name'},
            {text:'限制天数',value:'duration'},
            {text:'问题数量',value:'questions.length'},
            {text:'自动推送',value:'autopushed_str'},
            {text:'',value:'actions',sortable:false,align:'center'}
        ],
    }),
    methods:{
        expand: function(event){
            this.expanded=((this.expanded!==null&&this.expanded.length>0&&this.expanded[0].ttid===event.ttid)?[]:[event])
        },
        syncAPStr:function(event){
            event.autopushed_str=event.autopushed===true?"是":"否";
        },
        submitUnpush: function(){
            if(this.permission.pushReviews!==true || this.activeTmpl.autopushed===false)return;
            var vm=this;
            vm.fetchDelete('/api/review/template/'+this.activeTmpl.ttid+'/push')
            .then(function(res){
                if(res.status===204){
                    vm.activeTmpl.autopushed=false;
                    vm.syncAPStr(vm.activeTmpl);
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;  
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.unpushTmplDialog=false;
        },
        submitPush: function(){
            if(this.permission.pushReviews!==true || this.activeTmpl.autopushed===true)return;
            var vm=this;
            vm.fetchPostJson('/api/review/template/'+this.activeTmpl.ttid+'/push',{})
            .then(function(res){
                if(res.status===204){
                    vm.activeTmpl.autopushed=true;
                    vm.syncAPStr(vm.activeTmpl);
                    vm.$emit('push')
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;  
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.pushTmplDialog=false;
        },
        submitDelete: function(){
            if(this.permission.createReviewTemplate!==true || this.activeTmpl.autopushed===true)return;
            var vm=this;
            vm.fetchDelete('/api/review/template/'+this.activeTmpl.ttid)
            .then(function(res){
                if(res.status===204){
                    vm.templates.splice(vm.templates.indexOf(vm.activeTmpl),1);
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;  
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.deleteTmplDialog=false;
        },
        openPushDialog: function(event){
            if(this.permission.pushReviews!==true)return;
            this.activeTmpl=event;
            this.pushTmplDialog=true;
        },
        openUnpushDialog: function(event){
            if(this.permission.pushReviews!==true)return;
            this.activeTmpl=event;
            this.unpushTmplDialog=true;
        },
        openDeleteDialog: function(event){
            if(this.permission.createReviewTemplate!==true)return;
            this.activeTmpl=event;
            this.deleteTmplDialog=true;
        }
    }
}
</script>