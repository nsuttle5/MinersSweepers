using UnityEngine;

[CreateAssetMenu(fileName = "PatternRevealAbility", menuName = "Minesweeper/Abilities/PatternReveal")]
public class PatternRevealAbilitySO : SpawnableAbilitiesSO
{
    public int size = 3; //needs to be odd

    public bool[] revealPattern = new bool[9] { true, true, true, true, false, true, true, true, true }; //example

    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        int cx = revealedCell.x;
        int cy = revealedCell.y;
        int half = size / 2;

        for (int dx = -half; dx <= half; dx++)
        {
            for (int dy = -half; dy <= half; dy++)
            {
                int px = dx + half;
                int py = dy + half;
                int index = px + py * size;
                if (index < 0 || index >= revealPattern.Length)
                    continue;

                if (revealPattern[index])
                {
                    int nx = cx + dx;
                    int ny = cy + dy;
                    var cell = board.GetCellView(nx, ny);
                    if (cell != null && !cell.Revealed)
                        cell.Reveal();
                }
            }
        }
    }
}