module Logic
open SDLUtility
open System
open SDLGeometry

/////
//let noMoreParachuting = 450.0
//let miaow =  
//    let p  = new System.Media.SoundPlayer(@"..\..\..\..\images\cat_meow_x.wav")
//    fun () -> p.Play()
/////
let chaos = System.Random(DateTime.Now.Millisecond)

let screenWidth = 800.0
let screenHeight = 600.0

let fruitSize = 50.0
let headSize = 80.0
let neckIncreaseAmount = 10


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
        mutable Foods : ResizeArray<Position>
        mutable MaxFood : int
    }


let StartGame() =
    let state = 
        {            
            State = Playing
            NeckStart = Position.Zero()
            NeckAngle = 90.0
            NeckLength = 250
            Foods = ResizeArray<_>()
            MaxFood =  20
        }
    
    state

let overlap(rectA, rectB) =
    let x1 = rectA.X
    let x2 = rectA.X + rectA.Width
    let y1 = rectA.Y
    let y2 = rectA.Y + rectA.Height

    let x1' = rectB.X
    let x2' = rectB.X + rectB.Width
    let y1' = rectB.Y
    let y2' = rectB.Y + rectB.Height

    x2' >= x1 && x1' <= x2 && y2' >= y1 && y1' <= y2

let processCollisions (state:Game) =
    let mutable i = state.Foods.Count - 1
    while i > -1 do
        let food = state.Foods.[i]
        let r1 = { X = (int food.x) * 1<px>; Y = (int food.y) * 1<px>; Width = (int fruitSize) * 1<px>; Height = (int fruitSize) * 1<px>}
        let r2 = { X = (int state.NeckStart.x) * 1<px>; Y = (int state.NeckStart.y) * 1<px>; Width = (int headSize) * 1<px>; Height = (int headSize) * 1<px>}
        if overlap(r1, r2) then
            state.Foods.RemoveAt(i)
            state.NeckLength <- state.NeckLength + neckIncreaseAmount
        ()


let update (state:Game) =
    processCollisions state

    state
    
