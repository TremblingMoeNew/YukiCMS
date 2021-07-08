<template>
        <div >
            <div class="text-h5 mx-6 my-6">{{com!==null?com.name:"会场不存在"}}</div>
            <v-divider class="mx-9"/>
            <v-container fluid class="d-flex">
                <v-row justify="center" no-gutters>
                    <v-col order="1" order-md="3" lg="4" cols="12">
                        <v-card class="mx-2 my-2">
                            <v-card-title>席位</v-card-title>
                            <v-card-subtitle>您当前的席位为：</v-card-subtitle>
                            <v-card-text>
                                <p class="d-flex justify-center deep-orange--text accent-2" v-if="seat===null">尚未分配</p>
                                <p class="d-flex justify-center green--text darken-1" v-else>{{seat.name}}</p>
                            </v-card-text>
                        </v-card>
                        <v-card class="mx-2 my-2" v-if="seatsAssignedStatus!==null">
                            <v-card-title>席位分配</v-card-title>
                            <v-card-subtitle>当前席位分配情况</v-card-subtitle>
                            <v-card-text>
                                <p class="mx-3">未分配：</p>
                                <p class="d-flex justify-center deep-orange--text accent-2" >{{seatsAssignedStatus.unassigned}}</p>
                                <p class="mx-3">已分配：</p>
                                <p class="d-flex justify-center green--text darken-1">{{seatsAssignedStatus.assigned}}</p>
                            </v-card-text>
                        </v-card>
                        <v-card class="mx-2 my-2" v-if="reviewsCount!==null && (reviewsCount.reviewsByAdmActive>0 || reviewsCount.reviewsByAdmCompleted>0 || permission.Committee_General_Reviewer=== true)">
                            <v-card-title>学测概况</v-card-title>
                            <v-card-subtitle>当前学测情况</v-card-subtitle>
                            <v-card-text>
                                <p class="mx-3">由您担任学测官的正在进行中的学测/面试：
                                    <span class="deep-orange--text accent-2">{{reviewsCount.reviewsByAdmActive}}</span>
                                </p>
                                <p class="mx-3">由您担任学测官的已完成的学测/面试：
                                    <span class="green--text darken-1">{{reviewsCount.reviewsByAdmCompleted}}</span>
                                </p>
                                <template v-if="permission.Committee_General_Reviewer=== true">
                                    <p class="mx-3">未指定学测官的正在进行中的学测：
                                        <span class="deep-orange--text accent-2">{{reviewsCount.reviewsGeneralActive}}</span>
                                    </p>
                                    <p class="mx-3">学测/面试总计：
                                        <span class="green--text darken-1">{{reviewsCount.reviewsTotal}}</span>
                                    </p>
                                </template>
                            </v-card-text>
                        </v-card>
                    </v-col>
                    <v-col order="2" lg="auto" cols="12">
                        <v-divider class="d-none d-lg-flex" vertical/>
                        <v-divider class="d-flex d-lg-none my-5"/> 
                    </v-col>
                    <v-col order="3" order-md="1">
                        <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" v-if="reviewsActive===null||reviewsActive.length===0">您当前没有等待完成的学测</div>
                        <v-card v-for="(review,idx) of reviewsActive" :key="idx" class="mx-2 my-3"  @click="$router.push({name:'Committee-Review-Main',params:{cid:review.cid,tid:review.tid}})">
                            <v-card-text>
                                <p v-for="(question,j) of review.questions" :key="j" class="mx-3">{{j+1}}： {{question}}</p>
                            </v-card-text>
                            <v-card-subtitle>截止时间：{{review.ddl}}</v-card-subtitle>
                        </v-card>
                    </v-col>
                </v-row>
            </v-container>
        </div>
</template>
<script>
import mixin from '@/utils/mixins'
export default {
    name:"Committee-Main",
    mixins:[mixin],   
    props:['cid'],
    data:()=>({
        com:null,
        seat:null,
        permission:{
            Committee_Edit_InfoCommittee_Assign_Seats:true,
            Committee_Edit_InfoCommittee_General_Reviewer:true,
        },
        seatsAssignedStatus:null,
        reviewsCount:null,
        reviewsActive:[]
    }),
    watch:{
        com:function(ncom){
            if(ncom!==null){
                this.getSelfSeat();
                this.getSeatStatus();
                this.getReviewsCount();
                this.getReviewsActive();
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
        getSelfSeat:function(){
            var vm=this;
            this.fetchGet('/api/seat/c'+this.cid)
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.seat=json;
                    })
                }
            })
        },
        getSeatStatus:function(){
            var vm=this;
            this.fetchGet('/api/seat/c'+this.cid+'/status')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.seatsAssignedStatus=json;
                    })
                }
            })
        },
        getReviewsCount:function(){
            var vm=this;
            this.fetchGet('/api/review/c'+this.cid+'/count')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.reviewsCount=json;
                    })
                }
            })
        },
        getReviewsActive:function(){
            var vm=this;
            this.fetchGet('/api/review/c'+this.cid+'/self/active')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.initReviewsSelf(json);
                    })
                }
            })
        },
        initReviewsSelf:async function(rev){
            var vm=this;
            vm.statusCount=[0,0,0,0,0]
            vm.reviewsActive=[]
            for(var i=0;i<rev.length;i++){
                var review=Object.assign({},rev[i]);
                vm.statusCount[review.status]++;
                vm.reviewsActive.push(review);
            }
        }
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