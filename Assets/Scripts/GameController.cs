﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	// player
	private PlayerController player1;
	private PlayerController player2;

	// enemy
	public List <GameObject> enemycards;
	public GameObject enemy;
	private float enemy_start_x;
	private bool is_enemy_start;
	private Transform enemies;
	private GameObject enemycard;
	private List <Transform> enemies_in_card;
	private float enemycard_renew_time;
	private float enemycard_start_time;
	private float enemycard_destory_delay;

	// game control
	private GameObject hidden;
	private float title_time;
	private bool is_title_finished;
	private float option_time;
	private bool is_option_finished;
	private float game_start_time;
	public bool is_end;
	public bool is_multiplayer;

	// curtain
	private Transform curtain;
	private float curtain_speed;
	private float curtain_highest_position_y;
	private float curtain_lowest_position_y;

	// danger
	private Transform danger;
	private float danger_speed;
	private float danger_original_position_x;
	private float danger_final_position_x;
	private ParticleSystem danger_particle;
	private float original_lifetime;

	// tutorial
	private bool is_tutorial_set;
	private bool is_tutorial_finished;
	private Transform tutorial;
	private List <Transform> tutorial_texts;
	private float tutorial_speed;
	private float tutorial_text_start_position_x;
	private float tutorial_text_end_position_x;
	private float tutorial_close_x_start;
	private float tutorial_close_x_end;
	private float tutorial_close_speed;
	private bool skip_tutorial;
	private float color_speed;
	private string[] tutorial_strings;
	public GameObject tutorial_text;

	// game statistics
	private int kills;
	private int kills_0;
	private int kills_1;
	private int misses;
	private float distance;
	private int max_misses;
	private float distance_speed;
	private int highest;

	// UI
	public GameObject UI_kill;
	public GameObject UI_kill_0;
	public GameObject UI_kill_1;
	public GameObject UI_highest;
	public GameObject UI_title;
	public GameObject UI_option;

	// difficulty level
	private int difficulty_level;

	// Use this for initialization
	void Start () {
		// enemy
		enemies = transform.Find ("Enemies");
		enemy_start_x = 10f;
		is_enemy_start = false;
		enemycard_renew_time = 8f;
		enemycard_start_time = Time.time;
		enemycard_destory_delay = 20f;

		// game control
		title_time = 3f;
		is_title_finished = false;
		option_time = 3f;
		is_option_finished = false;
		game_start_time = Time.time;
		hidden = transform.Find ("Hidden").gameObject;
		if (hidden.activeSelf)
		{
			hidden.SetActive (false);
		}
		is_end = false;
		is_multiplayer = false;

		// curtain
		curtain = transform.Find ("Curtain");
		curtain_speed = 2f;
		curtain_highest_position_y = 4f;
		curtain_lowest_position_y = 0f;

		// danger
		danger = transform.Find ("Danger");
		danger_speed = 1f;
		danger_original_position_x = -5f;
		danger_final_position_x = -3.5f;
		danger.position = new Vector3 (danger_original_position_x, danger.position.y, danger.position.z);
		danger_particle = danger.GetComponent <ParticleSystem> ();
		original_lifetime = danger_particle.startLifetime;

		// game statistics
		kills = 0;
		kills_0 = 0;
		kills_1 = 0;
		misses = 0;
		max_misses = 50;
		distance = 0;
		distance_speed = 2f;
		highest = 0;

		// tutorials
		skip_tutorial = false;
		is_tutorial_set = false;
		is_tutorial_finished = false;
		tutorial_speed = 6f;
		tutorial_text_start_position_x = 6f;
		tutorial_text_end_position_x = -12f;
		tutorial_close_x_start = -0.5f;
		tutorial_close_x_end = -3f;
		tutorial_close_speed = 0.6f;
		color_speed = 0.3f;
		tutorial = transform.Find ("Tutorial");
		tutorial_texts = new List <Transform> ();

		/*
		foreach (Transform child in tutorial)
		{
			tutorial_texts.Add (child);
			child.position += Vector3.right * tutorial_text_start_position_x;
			child.gameObject.GetComponent <TextMesh> ().color = new Color (1f, 1f, 1f, 0);
		}
		*/

		// player controller
		player1 = transform.Find ("Player_0").gameObject.GetComponent <PlayerController> ();
		player2 = transform.Find ("Player_1").gameObject.GetComponent <PlayerController> ();
		player2.gameObject.SetActive (false);

		// UI
		HideUI ();
		HideHighest ();
		HideOption ();

		// difficulty level
		difficulty_level = 0;
	}

	// Update is called once per frame
	void Update () {
		if (!is_end && is_title_finished && !is_option_finished)
		{
			if (Input.GetKey ("s"))
			{
				is_multiplayer = false;
				is_option_finished = true;
				HideOption ();
			}
			else if (Input.GetKey ("down"))
			{
				is_multiplayer = true;
				player1.SetControl (true, false);
				player2.SetControl (false, true);
				player2.gameObject.SetActive (true);
				is_option_finished = true;
				HideOption ();
			}
		}

		if (!is_end && is_title_finished && is_option_finished && !is_tutorial_set)
		{
			is_tutorial_set = true;
			SetTutorial ();
		}

		if (!is_end && is_title_finished && is_option_finished && is_tutorial_set && !is_tutorial_finished)
		{
			if (Input.GetKey ("space"))
			{
				skip_tutorial = true;
			}
		}

		if (!is_end && is_title_finished && is_option_finished)
		{
			if (curtain.position.y < curtain_highest_position_y)
			{
				curtain.position += Vector3.up * Time.deltaTime * curtain_speed;
			}
			else if (curtain.position.y > curtain_highest_position_y)
			{
				curtain.position = new Vector3 (curtain.position.x, curtain_highest_position_y, curtain.position.z);
			}
			CheckControls ();
			UpdateTutorial ();
			UpdateStatue ();
			UpdateEnemies ();
		}
		else if (is_end && is_title_finished && is_option_finished) // is_end
		{
			if (curtain.position.y > curtain_lowest_position_y)
			{
				curtain.position += Vector3.down * Time.deltaTime * curtain_speed;
			}
			else if (curtain.position.y < curtain_lowest_position_y)
			{
				curtain.position = new Vector3 (curtain.position.x, curtain_lowest_position_y, curtain.position.z);
			}

			if (curtain.position.y == curtain_lowest_position_y)
			{
				if (Input.anyKey)
				{
					print ("anyKey");
					Init ();  // restart game
				}
			}
		}
		UpdateUI ();
	}

	void CheckControls ()
	{
		if (!is_end)
		{
			if (Input.GetKey ("1"))
			{
				difficulty_level = 1;
			}
			if (Input.GetKey ("2"))
			{
				difficulty_level = 2;
			}
			if (Input.GetKey ("3"))
			{
				difficulty_level = 3;
			}
			if (Input.GetKey ("4"))
			{
				difficulty_level = 4;
			}
			if (Input.GetKey ("5"))
			{
				difficulty_level = 5;
			}
		}
	}

	void UpdateStatue ()
	{
		if (!is_enemy_start && is_tutorial_finished)
		{
			is_enemy_start = true;
		}

		distance += Time.deltaTime * distance_speed;

		if (misses > 0 && danger.position.x < danger_final_position_x)
		{
			danger.position += Vector3.right * Time.deltaTime * danger_speed;
		}
		else if (danger.position.x != danger_final_position_x)
		{
			new Vector3 (danger_final_position_x, danger.position.y, danger.position.z);
		}

		if (danger.position.x != danger_final_position_x)
		{
			danger_particle.startLifetime = original_lifetime + Mathf.Round (misses / 5);
		}

		if (misses > max_misses)
		{
			End ();
		}
	}

	void UpdateEnemies ()
	{
		if (is_enemy_start)
		{
			// pick one enemycard
			if (enemycard == null)
			{
				Vector3 position = enemies.position;
				position.x += enemy_start_x;
				enemycard = Instantiate (enemycards[Random.Range (0, enemycards.Count-1)], position, enemies.rotation, enemies);
				enemies_in_card = new List <Transform> ();
				foreach (Transform child in enemycard.transform)
				{
					if (child.CompareTag (Tags.ENEMY))
					{
						enemies_in_card.Add (child);
					}
				}
				foreach (Transform child in enemies_in_card)
				{
					Instantiate (enemy, child.position, child.rotation, enemycard.transform);
					// enemies_in_card.Remove (child);
					Destroy (child.gameObject);
				}
				enemycard_start_time = Time.time;
			}
			else
			{
				if (Time.time - enemycard_start_time >= enemycard_renew_time)
				{
					Destroy (enemycard, enemycard_destory_delay);
					enemycard = null;
				}
			}
		}
	}

	void Init ()
	{
		is_end = false;
		danger.position = new Vector3 (danger_original_position_x, danger.position.y, danger.position.z);
		danger_particle = danger.GetComponent <ParticleSystem> ();
		original_lifetime = danger_particle.startLifetime;
		kills = 0;
		kills_0 = 0;
		kills_1 = 0;
		misses = 0;
		distance = 0;

		GameObject[] all_enemies = GameObject.FindGameObjectsWithTag (Tags.ENEMY);
		foreach (GameObject enemy in all_enemies)
		{
			Destroy (enemy);
		}

		UI_kill.GetComponent <Text> ().text = "Kills: " + kills;
		UI_kill_0.GetComponent <Text> ().text = "Fluffy: " + kills;
		UI_kill_1.GetComponent <Text> ().text = "Feathery: " + kills;

		player1.Init ();
		player2.Init ();
	}

	public void End ()
	{
		is_end = true;
	}

	public void AddKills ()
	{
		kills ++;
		UI_kill.GetComponent <Text> ().text = "Kills: " + kills;
		// update highest score
		highest = Mathf.Max (highest, kills);
		UI_highest.GetComponent <Text> ().text = "Highest: " + highest;
	}

	public void AddKills0 ()
	{
		kills_0 ++;
		UI_kill_0.GetComponent <Text> ().text = "Fluffy: " + kills_0;
	}

	public void AddKills1 ()
	{
		kills_1 ++;
		UI_kill_1.GetComponent <Text> ().text = "Feathery: " + kills_1;
	}

	public void AddMisses ()
	{
		misses ++;
	}

	void UpdateTutorial ()
	{
		if (!is_end && !is_tutorial_finished && !skip_tutorial && is_title_finished)
		{
			if (tutorial_texts.Count > 0)
			{
				Transform tutorial_text = tutorial_texts[0];
				if (tutorial_text.position.x > tutorial_text_end_position_x)
				{
					if (tutorial_text.position.x > tutorial_close_x_start)
					{
						tutorial_text.position += Vector3.left * Time.deltaTime * tutorial_speed;
					}
					else if (tutorial_text.position.x > tutorial_close_x_end)
					{
						tutorial_text.position += Vector3.left * Time.deltaTime * tutorial_close_speed;
						Color color = tutorial_text.gameObject.GetComponent <TextMesh> ().color;
						color.a += Time.deltaTime * color_speed;
						tutorial_text.gameObject.GetComponent <TextMesh> ().color = color;
					}
					else
					{
						tutorial_text.position += Vector3.left * Time.deltaTime * tutorial_speed;
						Color color = tutorial_text.gameObject.GetComponent <TextMesh> ().color;
						color.a -= Time.deltaTime * color_speed * 3;
						tutorial_text.gameObject.GetComponent <TextMesh> ().color = color;
					}
				}
				else
				{
					tutorial_texts.Remove (tutorial_text);
					Destroy (tutorial_text.gameObject);
				}
			}
			else
			{
				Destroy (tutorial.gameObject);
				is_tutorial_finished = true;
			}
		}

		if (!is_end && !is_tutorial_finished && skip_tutorial)
		{
			Destroy (tutorial.gameObject);
			is_tutorial_finished = true;
		}
	}

	public void UpdateUI ()
	{
		if (!is_title_finished && Time.time - game_start_time < title_time)
		{
			ShowTitle ();
		}
		else if (!is_title_finished)
		{
			is_title_finished = true;
			HideTitle ();
		}

		if (is_title_finished && !is_option_finished && Time.time - game_start_time - title_time < option_time)
		{
			ShowOption ();
		}
		else if (is_title_finished && !is_option_finished)
		{
			is_option_finished = true;
			HideOption ();
		}

		if (!is_end && is_tutorial_finished)
		{
			ShowUI ();
			HideHighest ();
		}

		if (is_end)
		{
			HideUI ();
			ShowHighest ();
		}
	}

	void HideUI ()
	{
		if (UI_kill.activeSelf)
		{
			UI_kill.SetActive (false);
		}
	}

	void ShowUI ()
	{
		if (!UI_kill.activeSelf)
		{
			UI_kill.SetActive (true);
		}
		if (is_multiplayer)
		{
			if (!UI_kill_0.activeSelf)
			{
				UI_kill_0.SetActive (true);
			}
			if (!UI_kill_1.activeSelf)
			{
				UI_kill_1.SetActive (true);
			}
		}
		else
		{
			if (UI_kill_0.activeSelf)
			{
				UI_kill_0.SetActive (false);
			}
			if (UI_kill_1.activeSelf)
			{
				UI_kill_1.SetActive (false);
			}
		}
	}

	void HideHighest ()
	{
		if (UI_highest.activeSelf)
		{
			UI_highest.SetActive (false);
		}
	}

	void ShowHighest ()
	{
		if (!UI_highest.activeSelf)
		{
			UI_highest.SetActive (true);
		}
	}

	void HideTitle ()
	{
		if (UI_title.activeSelf)
		{
			UI_title.SetActive (false);
		}
	}

	void ShowTitle ()
	{
		if (!UI_title.activeSelf)
		{
			UI_title.SetActive (true);
		}
	}

	void HideOption ()
	{
		if (UI_option.activeSelf)
		{
			UI_option.SetActive (false);
		}
	}

	void ShowOption ()
	{
		if (!UI_option.activeSelf)
		{
			UI_option.SetActive (true);
		}
	}

	public int GetDifficultyLevel ()
	{
		return difficulty_level;
	}

	void SetTutorial ()
	{
		if (is_multiplayer)
		{
			tutorial_strings = new string[7];
			tutorial_strings[0] = "Once upon a time, there live Fluffy and Feathery";
			tutorial_strings[1] = "They love to roll around and never get tired";
			tutorial_strings[2] = "Use WASD to control Fluffy";
			tutorial_strings[3] = "Use Arrows to control Feathery";
			tutorial_strings[4] = "Press W or Up to jump";
			tutorial_strings[5] = "Press D or Right to turn into a saw";
			tutorial_strings[6] = "One day, there come the Evil Spirits ...";
		}
		else
		{
			tutorial_strings = new string[5];
			tutorial_strings[0] = "Once upon a time, there lives Fluffy";
			tutorial_strings[1] = "It loves to roll around and never gets tired";
			tutorial_strings[2] = "Press W or Up to jump";
			tutorial_strings[3] = "Press D or Right to turn into a saw";
			tutorial_strings[4] = "One day, there come the Evil Spirits ...";
		}

		foreach (string tutorial_string in tutorial_strings)
		{
			GameObject new_tutorial_text = Instantiate (tutorial_text, tutorial);
			tutorial_texts.Add (new_tutorial_text.transform);
			new_tutorial_text.transform.position += Vector3.right * tutorial_text_start_position_x;
			new_tutorial_text.GetComponent <TextMesh> ().color = new Color (1f, 1f, 1f, 0);
			new_tutorial_text.GetComponent <TextMesh> ().text = tutorial_string;
		}
	}
}
