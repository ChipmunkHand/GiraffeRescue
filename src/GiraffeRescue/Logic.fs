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

let screenWidth = 800.0
let screenHeight = 600.0

let treePositionTop = 50  *  1<px>
let treePositionLeft =  50 * 1<px> 
let treeHeight = int screenHeight - 50
let treeWidth =  int screenWidth - 100

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
        mutable NeckStart : Position
        //mutable NeckEnd: Position
        mutable NeckLength : int
        mutable NeckAngle : float
        mutable Foods : Position list
        mutable MaxFood : int
    }


let StartGame() =
    let state = 
        {            
            State = Playing
            NeckStart = Position.Zero()
            NeckAngle = 90.0
            NeckLength = 250
            Foods = []
            MaxFood =  20
        }
    
    state


let update (state:Game) =
    

    state
    
