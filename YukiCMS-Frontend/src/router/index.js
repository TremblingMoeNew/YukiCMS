import Vue from 'vue'
import VueRouter from 'vue-router'
import Home from '../views/Home.vue'

Vue.use(VueRouter)

    const routes = [
    {
        path: '/',
        name: 'Home',
        component: Home
    },
    {
        path:'/login',
        name:'Login',
        component: () => import('../views/Login.vue'),
        meta:{
            beforeLogin:true
        }
    },
    {
        path:'/register',
        name:'Register',
        component: () => import('../views/Register.vue'),
        meta:{
            beforeLogin:true
        },
    },
    {
        path:'/forgetpassword',
        name:'Forget-Pass',
        component: () => import('../views/Forget-Pass.vue'),
        meta:{
            beforeLogin:true
        },
    },
    {
        path:'/forgetpassword/reset/:token',
        name:'Forget-Pass-Reset',
        component: () => import('../views/Forget-Pass-Reset.vue'),
        meta:{
            beforeLogin:true
        },
        props:true,
    },
    {
        path:'/info',
        name:"User-Info",
        component:()=>import('../views/User-Info-Edit.vue'),
    },
    {
        path:'/committee/apply',
        name:"Committee-Apply",
        component:()=>import('../views/Committees/Committee-Apply.vue'),
        meta:{
            afterRegister:true
        },
    },
    {
        path:'/committee/manage',
        name:"Committees-Management",
        component:()=>import('../views/Committees/Committees-Management.vue'),
    },
    {
        path:'/committee/manage/:cid(\\d+)',
        name:"Committee-Management",
        component:()=>import('../views/Committees/Committee-Management.vue'),
        props:true,
    },
    {
        path:'/committee/:cid(\\d+)',
        name:"Committee-Main",
        component:()=>import('../views/Committees/Committee-Main.vue'),
        props:true,
    },
    {
        path:'/committee/:cid(\\d+)/review',
        name:"Committee-Reviews-Main",
        component:()=>import('../views/Committees/Committee-Reviews-Main.vue'),
        props:true,
    },
    {
        path:'/committee/:cid(\\d+)/review/:tid(\\d+)',
        name:"Committee-Review-Main",
        component:()=>import('../views/Committees/Committee-Review-Main.vue'),
        props:true,
    },
    {
        path:'/committee/:cid(\\d+)/seats',
        name:"Committee-Seats-Management",
        component:()=>import('../views/Committees/Committee-Seats-Management.vue'),
        props:true,
    },


    {
        path:'/file',
        name:"Files-Main",
        component:()=>import('../views/Files-Main.vue')
    },
    {
        path:'/accommodation',
        name:"Accommodation-Main",
        component:()=>import('../views/Accommodation/Accommodation-Main.vue')
    },

    {
        path:'/payment',
        name:"Payments-Main",
        component:()=>import('../views/payments/Payments-Main.vue')
    },
    {
        path:'/payment/manage',
        name:"Payments-Management",
        component:()=>import('../views/payments/Payments-Management.vue')
    },
    {
        path:'/permission',
        name:'Permissions-Main',
        component:()=>import('../views/permissions/Permissions-Main.vue')
    },
    {
        path:'/permission/group',
        name:'Permission-Groups-Management',
        component:()=>import('../views/permissions/Permission-Groups-Management.vue')
    },
    {
        path:'/permission/group/:pgid(\\d+)',
        name:'Permission-Group-Management',
        component:()=>import('../views/permissions/Permission-Group-Management.vue'),
        props:true,
    },
    {
        path:'/advanced/global',
        name:'Global-Settings',
        component:()=>import('../views/Advanced/Global-Settings.vue')
    },
    {
        path:'/advanced/user',
        name:'Users-Management',
        component:()=>import('../views/Advanced/Users-Management.vue')
    },
    {
        path:'/advanced/user/:uid(\\d+)',
        name:'User-Management',
        component:()=>import('../views/Advanced/User-Management.vue'),
        props:true
    },
]

const router = new VueRouter({
    routes,
    mode: 'history'
})
router.beforeEach(async (to, from, next) => {

    if(router.app.$store.state.firstopen===true){
        router.app.$store.commit('loadFromLocalStorage');
        var res=await fetch(router.app.$store.state.backendUrl+'/api/user/name', {
            method: 'GET',
            headers: new Headers({
                'Authorization': router.app.$store.state.jwt,
            }),
        })
        if(res.status===200){
            var json=await res.json();
            router.app.$store.commit('setLoginUsr',json)
        }
        else{
            router.app.$store.commit('logout');
        }
    }
    if (to.meta.beforeLogin){
        if(router.app.$store.state.loginUsr===null)next();
        else next({ name: 'Home' })
    }
    else if(router.app.$store.state.loginUsr===null)next({
        name:'Login', 
        query: {redirect: to.path},
    })
    else if(router.app.$store.state.afterRegister===true&& to.meta.afterRegister!==true)next({
        name:'Committee-Apply',
    })
    else next();
    
})
export default router
