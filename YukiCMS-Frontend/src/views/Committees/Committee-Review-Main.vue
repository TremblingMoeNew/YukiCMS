<template>
    <div>
        <div class="text-h5 mx-6 mt-6">{{com!==null?com.name:"会场不存在"}} / 学测</div>
        <template v-if="review===null || (loginUsr.uid!=review.uid && loginUsr.uid!=review.admUid&& (permission.generalReviewer!==true)  )">
            <v-divider class="mx-9 mt-6"/>
            <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" >
                您没有查看的权限
            </div>
        </template>
        <template v-else>
            <div class="d-flex align-center mx-9 my-1">
                <v-spacer />
                <v-btn text class="mx-2" color="primary" v-if="isThisReviewee===true && const_Review_Status[review.status].canSubmit===true" @click="submitReviewDialog=true;">提交学测</v-btn>
                <template v-else-if="isThisReviewer===true && const_Review_Status[review.status].canComplete===true">
                    <v-btn text class="mx-2" color="green darken-1"  @click="completeReviewDialog=true;">通过学测</v-btn>
                    <v-btn text class="mx-2" color="deep-orange accent-2" v-if="permission.cancelReviews===true" @click="cancelReviewDialog=true">关闭学测</v-btn>
                </template>
                <div v-else class="mt-6" />
            </div>
            <v-divider class="mx-9"/>
            <v-container fluid class="d-flex">
                <v-row justify="center" no-gutters>
                    <v-col order="1" order-md="3" md="4" cols="12">
                        <v-card class="mx-2 my-2" v-if="review!==null">
                            <v-card-title>学测状态</v-card-title>
                            <v-card-text>
                                <p>状态：</p>
                                <p class="d-flex justify-center" :class="const_Review_Status[review.status].color">
                                    {{const_Review_Status[review.status].text}}
                                </p>
                                <p>提交截止时间：</p>
                                <p class="d-flex justify-center">
                                    {{review.ddl}}
                                </p>

                            </v-card-text>
                        </v-card>
                        <v-card class="mx-2 my-2" v-if="reviewee!==null" >
                            <v-card-title>代表信息</v-card-title>
                            <v-card-text>
                                <p class="mx-3 my-1">姓名：{{reviewee.name}}</p>
                                <p class="mx-3 my-1">性别：{{reviewee.sex}}</p>
                                <p class="mx-3 my-1">邮箱：{{reviewee.email}}</p>
                                <p class="mx-3 my-1">电话号码：{{reviewee.phoneNumber}}</p>
                                <p class="mx-3 my-1">QQ号：{{reviewee.qqNumber}}</p>
                                <p class="mx-3 my-1">微信号：{{reviewee.wechatNumber}}</p>
                            </v-card-text>
                        </v-card>
                        <v-card class="mx-2 my-2" v-if="reviewer!==null">
                            <v-card-title>学测官信息</v-card-title>
                            <v-card-text>
                                <p class="mx-3 my-1">姓名：{{reviewer.name}}</p>
                                <p class="mx-3 my-1">性别：{{reviewer.sex}}</p>
                                <p class="mx-3 my-1">邮箱：{{reviewer.email}}</p>
                                <p class="mx-3 my-1">电话号码：{{reviewer.phoneNumber}}</p>
                                <p class="mx-3 my-1">QQ号：{{reviewer.qqNumber}}</p>
                                <p class="mx-3 my-1">微信号：{{reviewer.wechatNumber}}</p>
                            </v-card-text>
                        </v-card>
                    </v-col>
                    <v-col order="2" md="auto" cols="12">
                        <v-divider class="d-none d-lg-flex" vertical/>
                        <v-divider class="d-flex d-lg-none my-5"/> 
                    </v-col>
                    <v-col order="3" order-md="1">
                        <div class="mx-3 mt-4 mb-6" v-if="isVisibleReviewer===true">
                            <v-card class="my-6">
                                <v-card-title>
                                    点评
                                </v-card-title>
                                <v-card-text>
                                    <v-textarea v-model="review.comment" label="点评" dense auto-grow outlined class="mx-md-5 my-3" @input="debouncedUpdateComment()" :readonly="isThisReviewer!==true"/>
                                    <div class="d-flex align-center mx-md-5 fade-transition">
                                        <v-fade-transition>
                                            <span v-if="commentsaving===true">
                                                <span class="mx-2 deep-orange--text accent-2" v-if="commentfailed===true">自动保存失败，请检查您的网络连接或重新登录</span>
                                                <span v-else>
                                                    <v-progress-circular indeterminate size="25"/>
                                                    <span class="mx-2">自动保存中……</span>
                                                </span>
                                            </span>
                                        </v-fade-transition>
                                    </div>
                                </v-card-text>
                            </v-card>
                            <v-divider />
                        </div>
                        <div class="mx-3 my-4">
                            <p class="text-h6 mx-2 d-flex justify-start">
                                问题
                            </p>
                            <v-card v-for="(question,idx) of questions" :key="idx" class="mx-2 my-2">
                                <v-card-subtitle class="text-h6">问题{{idx+1}}：</v-card-subtitle>
                                <v-card-text>
                                    <pre class="text-subtitle-1 mx-md-5" style="white-space: pre-wrap; word-wrap: break-word;">{{question.question}}</pre>
                                    <v-textarea v-model="question.answer" label="回答" dense auto-grow outlined class="mx-md-5 my-3" @input="debouncedUpdateAnswer(question)" :readonly="isThisReviewee!==true"/>
                                    <div class="d-flex align-center mx-md-5 fade-transition">
                                        <v-fade-transition>
                                            <span v-if="question.saving===true">
                                                <span class="mx-2 deep-orange--text accent-2" v-if="question.failed===true">自动保存失败，请检查您的网络连接或重新登录</span>
                                                <span v-else>
                                                    <v-progress-circular indeterminate size="25"/>
                                                    <span class="mx-2">自动保存中……</span>
                                                </span>
                                            </span>
                                        </v-fade-transition>
                                        <v-spacer />
                                        <v-btn class="mx-2" outlined v-if="isThisReviewee===true" @click="openUploadFileDialog(idx)" color="deep-orange accent-2">上传附件</v-btn>
                                        <v-btn class="mx-2" outlined v-if="question.attachment!==null &&(isThisReviewee===true || isVisibleReviewer===true)" @click.stop="downloadFile(question.qid)" color="primary">下载附件</v-btn> 
                                    </div>
                                </v-card-text>
                            </v-card>
                        </div>
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
            <v-dialog v-model="DownloadFileDialog" width="400" persistent>
                <v-card>
                    <v-card-title>下载文件</v-card-title>
                    <v-card-text>
                        <div class="mx-12">
                            <p class="d-flex justify-center">下载中……</p>
                            <v-progress-linear indeterminate/>
                        </div>
                    </v-card-text>
                </v-card>
            </v-dialog>
            <v-dialog v-model="uploadFileDialog" width="400" :persistent="ufdPersistent">
                <v-card>
                    <v-card-title>上传文件</v-card-title>
                    <v-card-subtitle>点击以上传文件；上传的文件将替换原有的附件</v-card-subtitle>
                    <v-card-text>
                        <p v-if="uploadsizeExceeded" class="d-flex justify-center deep-orange--text accent-2">文件大小不得大于12MB</p>
                        <div class="mx-12" v-if="uploading===true">
                            <p class="d-flex justify-center">上传中……</p>
                            <v-progress-linear indeterminate/>
                        </div>
                        <v-file-input v-model="uploadvalue" @change="uploadFile" show-size label="上传文件"/>
                    </v-card-text>
                </v-card>
            </v-dialog>
            <v-dialog v-model="submitReviewDialog" width="400">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">
                            您确定要 <span class="primary--text">提交</span> 学测吗？
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="submitReviewDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" :disabled="review.uid!==loginUsr.uid || const_Review_Status[review.status].canSubmit!==true" @click="submitReview();">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="completeReviewDialog" width="400">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">
                            您确定要 <span class="green--text darken-1">通过</span> 学测吗？
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="completeReviewDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" :disabled="isThisReviewer!==true || const_Review_Status[review.status].canComplete!==true" @click="completeReview();">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="cancelReviewDialog" width="400">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">
                            您确定要 <span class="deep-orange--text accent-2">关闭</span> 学测吗？
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="cancelReviewDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" :disabled="isThisReviewer!==true || const_Review_Status[review.status].canComplete!==true" @click="cancelReview();">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
        </template>
    </div>
</template>
<script>
import lodash from 'lodash'
import mixin from '@/utils/mixins'
export default {
    name:"Committee-Review-Main",
    props:['cid','tid'],
    mixins:[mixin],       
    data:()=>({
        DownloadFileDialog:null,
        ufdPersistent:false,
        uploadidx:null,
        uploadFileDialog:null,
        uploadvalue:null,
        uploadsizeExceeded:false,
        uploading:false,
        submitReviewDialog:null,
        completeReviewDialog:null,
        cancelReviewDialog:null,
        commentsaving:false,
        commentfailed:false,
        com:null,
        notedialog:{
            active:null,
            success:true,
        },
        review:null,
        reviewee:null,
        reviewer:null,
        questions:[],
        permission:{
            generalReviewer:false,
            cancelReviews:false,
        },
        const_Review_Status:[
            {text:"未完成",color:"deep-orange--text accent-2",canSubmit:true,canComplete:true,},
            {text:"已提交",color:"blue--text lighten-4",canSubmit:false,canComplete:true,},
            {text:"已完成",color:"green--text darken-1",canSubmit:false,},
            {text:"已过期",color:"deep-purple--text darken-1",canSubmit:true,canComplete:true,},
            {text:"已关闭",color:"grey--text darken-1",canSubmit:false,},
        ],
    }),
    computed:{
        isThisReviewee:function(){
            return this.review.uid===this.loginUsr.uid
        },
        isThisReviewer:function(){
            return (this.review.admUid===this.loginUsr.uid || (this.review.admUid===0 && this.permission.generalReviewer===true))
        },
        isVisibleReviewer:function(){
            return (this.review.admUid===this.loginUsr.uid || this.permission.generalReviewer===true)
        }
    },
    watch:{
        com:function(ncom){
            if(ncom!==null){
                this.getReview();
            }
        },
        review:function(nrev){
            if(nrev!==null){
                this.getPermission();
                this.getQuestions();
                this.getReviewer();
                this.getReviewee();
            }
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
        getReview:function(){
            var vm=this;
            this.fetchGet('/api/review/'+this.tid)
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.review=json;
                    })
                }
            })
        },
        getReviewer:function(){
            var vm=this;
            this.fetchGet('/api/review/'+this.tid+'/reviewer')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.reviewer=json;
                    })
                }
            })
        },
        getReviewee:function(){
            var vm=this;
            this.fetchGet('/api/review/'+this.tid+'/reviewee')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.reviewee=json;
                    })
                }
            })
        },
        getQuestions:async function(){
            var vm=this;
            this.questions=[];
            for(var i=0;i<vm.review.questions.length;i++){
                var qid=vm.review.questions[i];
                var res=await vm.fetchGet('/api/review/'+vm.tid+'/question/'+qid);
                if(res.status===200){
                    var q=await res.json();
                    q.saving=false;
                    q.failed=false;
                    vm.questions.push(q);
                }
            }
        },
        updateAnswer:function(question){
            question.failed=false;
            question.saving=true;
            var vm=this;
            vm.fetchPostJson('/api/review/question/'+question.qid+'/answer',question.answer)
            .then(function(res){
                if(res.status===204){
                    vm.debouncedHideASHint(question);
                }
                else{
                    question.failed=true;
                    vm.debouncedHideASHintLong(question);
                }
            })
            
        },
        updateComment:function(){
            this.commentfailed=false;
            this.commentsaving=true;
            var vm=this;
            vm.fetchPutJson('/api/review/'+this.tid+'/comment',this.review.comment)
            .then(function(res){
                if(res.status===204){
                    vm.debouncedHideCASHint();
                }
                else{
                    vm.commentfailed=true;
                    vm.debouncedHideCASHintLong();
                }
            })
        },
        hideCASHint:function(){
            this.commentfailed=false;
            this.commentsaving=false;
        },
        hideASHint:function(question){
            question.saving=false;
            question.failed=false;
        },
        downloadFile:async function(qid){
            if(qid===null)return;
            this.DownloadFileDialog=true;
            var nameresp=await this.fetchGet('/api/review/'+this.tid+'/question/'+qid+'/attachment/name');
            var fn="";
            if(nameresp.status==200){
                var j= await nameresp.json();
                fn=j.fileOriginalName;
            }
            var suc=await this.fetchDownload('/api/review/'+this.tid+'/question/'+qid+'/attachment',fn)
            if(suc!==true){
                this.notedialog.success=false;
                this.notedialog.active=true;
            }
            this.DownloadFileDialog=false;
        },
        fetchDownload:async function(url,filename){
            let response = await this.fetchGet(url);
            if(response.status!==200)return false;
            var blob=await response.blob();
            const link = document.createElement('a')
            link.download = decodeURIComponent(filename)
            link.style.display = 'none'
            link.href = URL.createObjectURL(blob)
            link.onload=function(){
                URL.revokeObjectURL(link.href)
            }
            link.click()
            return true;
        },
        openUploadFileDialog:function(idx){
            this.uploadidx=idx;
            this.uploadFileDialog=true;
        },
        uploadFile:function(event){
            this.uploadsizeExceeded=event.size>1024*1024*12;
            if(this.uploadsizeExceeded===true)return;
            this.ufdPersistent=true;
            this.uploading=true;
            var vm=this;
            this.fetchUploadPost('/api/review/'+this.tid+'/question/'+this.questions[vm.uploadidx].qid+'/attachment',event)
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.questions[vm.uploadidx].attachment=json.name
                    })
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=false;
                }
                vm.uploading=false;
                vm.ufdPersistent=false;
                vm.uploadFileDialog=false;
            })
        },
        submitReview:function(){
            var vm=this;
            vm.fetchPostJson('/api/review/'+this.tid,{})
            .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.review.status=1
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=false;
                }
            })
            this.submitReviewDialog=false;
        },
        completeReview:function(){
            var vm=this;
            vm.fetchPutJson('/api/review/'+this.tid,{})
            .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.review.status=2
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=false;
                }
            })
            this.completeReviewDialog=false;
        },
        cancelReview:function(){
            var vm=this;
            vm.fetchDelete('/api/review/'+this.tid)
            .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.review.status=4
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=false;
                }
            })
            this.cancelReviewDialog=false;
        }

    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getCommittee())
    },
    beforeRouteUpdate (to, from, next) {
        this.getCommittee();
        next()
    },
    created() {
        this.debouncedUpdateAnswer = lodash.debounce(this.updateAnswer, 5000)
        this.debouncedHideASHint = lodash.debounce(this.hideASHint, 3000)
        this.debouncedHideASHintLong = lodash.debounce(this.hideASHint, 10000)
        this.debouncedUpdateComment = lodash.debounce(this.updateComment,5000)
        this.debouncedHideCASHint = lodash.debounce(this.hideCASHint, 3000)
        this.debouncedHideCASHintLong = lodash.debounce(this.hideCASHint, 10000)
    },
}
</script>