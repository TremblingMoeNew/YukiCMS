<template>
    <div>
        <div class="text-h5 mx-6 mt-6">{{com!==null?com.name:"会场不存在"}} / 学测</div>
        <div class="d-flex align-center mx-9">
            <v-tabs v-model="tab" right center-active show-arrows v-if="permission!==null && (permission.createReviews===true||permission.generalReviewer===true || permission.cancelReviews===true || permission.pushReviews===true ||  permission.createReviewTemplate===true)">
                <v-tab>我的学测</v-tab>
                <v-tab v-if="permission.generalReviewer===true || permission.createReviews===true">管理学测</v-tab>
                <v-btn text class="grey--text darken-2 align-self-center" @click.stop="createReviewDialog=true" v-if="permission.createReviews===true">创建学测</v-btn>
                <v-tab v-if="permission.pushReviews===true ||  permission.createReviewTemplate===true">管理模板</v-tab>
                <v-btn text class="grey--text darken-2 align-self-center" @click.stop="createTmplDialog=true" v-if="permission.createReviewTemplate===true">创建模板</v-btn>
            </v-tabs>
            <div v-else class="mt-6"/>
        </div>
        <v-divider class="mx-9 "/>
        <v-tabs-items v-model="tab" class="mx-lg-12 my-3">
            <v-tab-item>
                <Y-committee-reviews-self :reviewsSelf="reviewsSelf"/>
            </v-tab-item>
            <v-tab-item v-if="permission!==null &&(permission.generalReviewer===true || permission.createReviews===true)">
                <Y-committee-reviews-management :reviewsAsReviewer="reviewsAsReviewer" :rga="reviewsGeneralReviewer" :rarchived="reviewsArchived" :permission="permission" />
            </v-tab-item>
            <v-tab-item v-if="permission!==null &&(permission.pushReviews===true ||  permission.createReviewTemplate===true)">
                <Y-committee-review-tmpls-management :com="com" v-model="tmpls" :permission="permission" @push="getReviewsSelf();getReviewsForManagement()"/>
            </v-tab-item>
        </v-tabs-items>
        <v-dialog v-model="notedialog.active" width="200">
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text>
                    <p v-if="notedialog.success===true" class="d-flex justify-center">操作成功！</p>
                    <p v-else class="d-flex justify-center">操作失败！</p>
                </v-card-text>
            </v-card>
        </v-dialog>
        <v-dialog v-model="createTmplDialog" width="600" v-if="permission!==null && permission.createReviewTemplate===true">
            <v-card width="600">
                <v-toolbar flat dense color="blue lighten-4">
                    <v-toolbar-title>创建学测模板</v-toolbar-title>
                    <v-spacer></v-spacer>
                </v-toolbar>
                <v-card-text>
                    <v-form class="mx-5 mt-3">
                        <v-container>
                            <v-row dense>
                                <v-col cols="12" sm="6"><v-text-field label="模板名" v-model="new_tmpl.name"/></v-col>
                                <v-col cols="12" sm="6"><v-text-field label="限制天数" v-model="new_tmpl.duration"/></v-col>
                            </v-row>
                            <v-row v-for="(question,idx) of new_tmpl.questions" :key="idx" dense>
                                <v-col>
                                    <v-textarea v-model="new_tmpl.questions[idx]" outlined rows="2" dense auto-grow :label="'问题'+(idx+1)" />
                                </v-col>
                            </v-row>
                            <v-row dense>
                                <v-col/>
                                <v-col cols="auto"> 
                                    <v-btn outlined color="deep-orange accent-2" @click.stop="reduceQuestionTmpl()" :disabled="new_tmpl.questions.length===0">- 减少问题</v-btn>
                                </v-col>
                                <v-col/>
                                <v-col cols="auto"> 
                                    <v-btn outlined color="deep-orange accent-2" @click.stop="addQuestionTmpl()" :disabled="CRTAddEnabled!==true">+ 添加问题</v-btn>
                                </v-col>
                                <v-col/>
                            </v-row>
                        </v-container>
                    </v-form>
                </v-card-text>
                <v-card-actions>
                    <v-spacer />
                    <v-btn class="mx-2" color="blue lighten-4" :disabled="new_tmpl.questions.length === 0" @click="submitCreateTmpl();">创建</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog v-model="createReviewDialog" width="600" v-if="permission!==null && permission.createReviews===true">
            <v-card width="600">
                <v-toolbar flat dense color="blue lighten-4">
                    <v-toolbar-title>创建学测</v-toolbar-title>
                    <v-spacer></v-spacer>
                </v-toolbar>
                <v-card-text>
                    <v-form class="mx-5 mt-3">
                        <v-container>
                            <v-row dense>
                                <v-col md="6" cols="12">
                                    <v-select :items="users" item-text="name" label="代表" item-value="uid" v-model="new_review.uid"/>
                                </v-col>
                            </v-row>
                            <v-row dense>
                                <v-col md="6" cols="12">
                                    <v-select :items="admUsers" item-text="name" label="学测官" item-value="uid" v-model="new_review.admUid"/>
                                </v-col>
                                <v-col md="6" cols="12" class="d-flex justify-center">
                                    <v-switch label="从模板创建" v-model="new_review.buildfromTmpl"/>
                                </v-col>
                            </v-row>
                            <template v-if="new_review.buildfromTmpl===true">
                                <v-row>
                                    <v-col md="6" cols="12">
                                        <v-select :items="tmpls" item-text="name" label="模板" item-value="ttid" return-object v-model="new_review.tmpl"/>
                                    </v-col>
                                    <v-col md="6" cols="12" v-if="new_review.tmpl.duration!==null">
                                        <v-text-field v-model="new_review.tmpl.duration" readonly label="限制天数"/>
                                    </v-col>
                                </v-row>
                                <v-row v-for="(question,idx) of new_review.tmpl.questions" :key="idx">
                                    <v-col>
                                        <v-textarea :value="question" outlined rows="2" dense auto-grow :label="'问题'+(idx+1)" readonly/>
                                    </v-col>
                                </v-row> 
                            </template>
                            <template v-else>
                                <v-row>
                                    <v-col md="6" cols="12">
                                        <v-text-field v-model="new_review.duration" label="限制天数"/>
                                    </v-col>
                                </v-row>
                                <v-row v-for="(question,idx) of new_review.questions" :key="idx" dense>
                                    <v-col>
                                        <v-textarea v-model="new_review.questions[idx]" outlined rows="2" dense auto-grow :label="'问题'+(idx+1)" />
                                    </v-col>
                                </v-row>
                                <v-row dense>
                                    <v-col/>
                                    <v-col cols="auto"> 
                                        <v-btn outlined color="deep-orange accent-2" @click.stop="reduceQuestionReview()" :disabled="new_review.questions.length===0">- 减少问题</v-btn>
                                    </v-col>
                                    <v-col/>
                                    <v-col cols="auto"> 
                                        <v-btn outlined color="deep-orange accent-2" @click.stop="addQuestionReview()" :disabled="CRQAddEnabled!==true">+ 添加问题</v-btn>
                                    </v-col>
                                    <v-col/>
                                </v-row>
                            </template>
                        </v-container>
                    </v-form>
                </v-card-text>
                <v-card-actions>
                    <v-spacer />
                    <v-btn class="mx-2" color="blue lighten-4" :disabled="CRQSubmitEnabled!==true" @click="submitCreateReview();createReviewDialog=false">创建</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>
<script>
import YCommitteeReviewsSelf from '@/components/Committee-Reviews-Self'
import YCommitteeReviewsManagement from '@/components/Committee-Reviews-Management'
import YCommitteeReviewTmplsManagement from '@/components/Committee-Review-Tmpls-Management'
import mixin from '@/utils/mixins'
export default {
    name:"Committee-Reviews-Main",
    mixins:[mixin],
    props:['cid'],
    components:{
        YCommitteeReviewsSelf,
        YCommitteeReviewsManagement,
        YCommitteeReviewTmplsManagement
    },
    data:()=>({
        tab:0,
        createReviewDialog:null,
        createTmplDialog:null,
        com:null,
        new_review:{
            buildfromTmpl:null,
            admUid:null,
            ttid:null,
            questions:[],
            duration:null,
            tmpl:{duration:null,questions:[]},
        },
        new_tmpl:{questions:[]},
        notedialog:{
            active:null,
            success:true,
        },
        permission:null,
        users:[],
        admUsers:[],
        tmpls:[],
        reviewsSelf:[],
        reviewsAsReviewer:[],
        reviewsGeneralReviewer:[],
        reviewsArchived:[],
    }),
    watch:{
        com:function(ncom){
            if(ncom!==null){
                this.getPermission();
                this.getReviewsSelf();
            }
        },
        permission:function(nper) {
            if(nper!==null){
                if(this.permission.createReviews===true || this.permission.cancelReviews===true ){
                    this.getReviewers();
                    this.getTemplates();
                    this.getMembers();
                }
                if(this.permission.pushReviews===true || this.permission.createReviewTemplate===true){
                    this.getTemplates();
                }
                if(this.permission.generalReviewer===true){
                    this.getReviewsForManagement();
                }
            }
        }
    },
    computed:{
        CRQAddEnabled:function(){
            var len=this.new_review.questions.length;
            if(len===0)return true;
            var q=this.new_review.questions[len-1];
            if(q===null||q==="")return false;else return true;
        },
       CRTAddEnabled:function(){
            var len=this.new_tmpl.questions.length;
            if(len===0)return true;
            var q=this.new_tmpl.questions[len-1];
            if(q===null||q==="")return false;else return true;
        },
        CRQSubmitEnabled:function(){
            if(this.new_review.admUid===null)return false;
            if(this.new_review.buildfromTmpl===true){
                if(this.new_review.tmpl.duration===undefined)return false;
            }else{
                if(this.new_review.duration===null)return false;
                if(this.CRQAddEnabled!==true)return false;
                if(this.new_review.questions.length===0)return false;
            }
            return true;
        }
    },
    methods:{
        getCommittee:function(){
            var vm=this;
            this.fetchGet('/api/committee/c'+this.cid+'/brief')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.com=json;
                    })
                }
            })
        },
        getPermission:function() {
            var vm=this;
            this.fetchGet('/api/permission/vertify/committee/review/'+this.cid)
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.permission=json;
                    })
                }
            })
        },
        getTemplates:function(){
            var vm=this;
            this.fetchGet('/api/review/template/c'+this.cid)
            .then(function(res){
                if(res.status===200){
                    res.json().then(async function(json){
                        await vm.initInjectAutoPushedDataToTmpls(json)
                        vm.tmpls=json;
                    })
                }
            })
        },
        getMembers:function(){
            var vm=this;
            this.fetchGet('/api/committee/c'+this.cid+'/members')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.users=json;
                    })
                }
            })
        },
        getReviewers:function(){
            var vm=this;
            this.fetchGet('/api/review/c'+this.cid+'/reviewer')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.admUsers=json;
                    })
                }
            })
        },
        getReviewsSelf:function(){
            var vm=this;
            this.fetchGet('/api/review/c'+this.cid+'/self')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.reviewsSelf=json;
                    })
                }
            })
        },
        getReviewsAsReviewer:function(){
            var vm=this;
            this.fetchGet('/api/review/c'+vm.cid)
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.reviewsAsReviewer=json;
                    })
                }
            })
        },
        getReviewsGeneralReviewer:function(){
            var vm=this;
            this.fetchGet('/api/review/c'+vm.cid+'/general')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.reviewsGeneralReviewer=json;
                    })
                }
            })
        },
        getReviewsArchived:function(){
            var vm=this;
            this.fetchGet('/api/review/c'+vm.cid+'/archived')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.reviewsArchived=json;
                    })
                }
            })
        },
        getReviewsForManagement:function(){
            this.getReviewsAsReviewer();
            this.getReviewsGeneralReviewer();
            this.getReviewsArchived();
        },
        submitCreateReview:function(){
                // TODO
            var vm=this;
            var rev={
                admUid:this.new_review.admUid,
            }
            if(this.new_review.buildfromTmpl===true){
                rev.duration=this.new_review.tmpl.duration,
                rev.questions=this.new_review.tmpl.questions
            }
            else{
                rev.duration=this.new_review.duration,
                rev.questions=this.new_review.questions;
            }
            vm.fetchPostJson('/api/review/c'+this.cid+'/u'+this.new_review.uid,rev)
            .then(function(res){
                if(res.status===201){
                    vm.getReviewsSelf();
                    vm.getReviewsForManagement();
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.new_review={
                        buildfromTmpl:null,
                        admUid:null,
                        ttid:null,
                        questions:[],
                        duration:null,
                        tmpl:{duration:null,questions:[]},
                    };
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                    vm.new_review={
                        buildfromTmpl:null,
                        admUid:null,
                        ttid:null,
                        questions:[],
                        duration:null,
                        tmpl:{duration:null,questions:[]},
                    };
                }
            })
            this.new_review={
                buildfromTmpl:null,
                admUid:null,
                ttid:null,
                questions:[],
                duration:null,
                tmpl:{duration:null,questions:[]},
            };
            this.notedialog.active=true;
        },
        addQuestionReview:function(){
            this.new_review.questions.push(null)
        },
        reduceQuestionReview:function(){
            this.new_review.questions.pop()
        },
        
        submitCreateTmpl:function(){
            var vm=this;
            this.new_tmpl.cid=this.cid;
            vm.fetchPostJson('/api/review/template/c'+this.cid,this.new_tmpl)
            .then(function(res){
                if(res.status===201){
                    res.json().then(function(json){
                        json.autopushed=false;
                        json.autopushed_str='否'
                        vm.tmpls.push(json)
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                        vm.new_tmpl={questions:[]};
                    })
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                    vm.new_tmpl={questions:[]};
                }
            })
            this.createTmplDialog=false;
        },
        addQuestionTmpl:function(){
            this.new_tmpl.questions.push(null)
        },
        reduceQuestionTmpl:function(){
            this.new_tmpl.questions.pop()
        },
        initInjectAutoPushedDataToTmpls:async function(tmpls){
            var res=await this.fetchGet('/api/committee/c'+this.cid+'/push')
            if(res.status===200){
                var autopushedTasks=await res.json()
                for(var i=0;i<tmpls.length;i++)
                {
                    var item=tmpls[i]
                    if(autopushedTasks.indexOf(item.ttid)>=0)
                    {
                        item.autopushed=true;
                        item.autopushed_str='是';
                    }
                    else
                    {
                        item.autopushed=false;
                        item.autopushed_str='否';
                    }
                }
            }

        },
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getCommittee())
    },
    beforeRouteUpdate (to, from, next) {
        this.getCommittee();
        next()
    },
}
</script>