module Logic
open SDLUtility
open System
/////
//let noMoreParachuting = 450.0
//let miaow =  
//    let p  = new System.Media.SoundPlayer(@"..\..\..\..\images\cat_meow_x.wav")
//    fun () -> p.Play()
/////
let chaos = System.Random(DateTime.Now.Millisecond)

let screenWidth = 800.0;
let screenHeight = 600.0;

type Size = 
    {
        width : float
        height : float 
    }

type Position = 
    {
        mutable x : float
        mutable y : float
        mutable vx : float
        mutable vy : float
    } 
    with 
    static member Zero() =
        { x = 0.; y = 0.; vx=0.; vy = 0. }
    member this.Update() = 
            this.x <- this.x + this.vx
            this.y <- this.y + this.vy

type GameState =
    | Playing
    | GameOver
           
type Game =
    {
        mutable State : GameState
    }


let StartGame() =
    let state = 
        {            
            State = Playing
        }
    
    state


let update (state:Game) =
    state
    
